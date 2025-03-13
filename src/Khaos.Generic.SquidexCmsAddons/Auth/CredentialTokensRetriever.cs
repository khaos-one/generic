using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Text.Json;

namespace Khaos.Generic.SquidexCmsAddons.Auth;

public interface ICredentialTokensRetriever
{
    Task<CredentialTokens> GetCredentialTokensAsync(CancellationToken cancellationToken = default);
}

public sealed class CredentialTokensRetriever(
    IOptions<Options> options, 
    IHttpClientFactory httpClientFactory,
    IMemoryCache cache) 
    : ICredentialTokensRetriever
{

    private readonly HttpClient _httpClient = httpClientFactory.CreateSquidexTokenResolverHttpClient();
    private readonly Options _options = options.Value;
    

    public async Task<CredentialTokens> GetCredentialTokensAsync(CancellationToken cancellationToken = default)
    {
        var cachedTokens = TryGetTokensFromCache();
        
        if (cachedTokens != null)
        {
            return cachedTokens;
        }

        var result = await GetTokensFromApi();
        SetTokensToCache(result);
        
        return result;
    }

    private static string GenerateCodeVerifier()
    {
        var bytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string verifier)
    {
        using var sha256 = SHA256.Create();
        var challengeBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(verifier));
        return Convert.ToBase64String(challengeBytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private Uri GetAuthorizeUri(string codeChallenge)
    {
        var authorizeParams = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["response_type"] = "code",
            ["scope"] = "squidex-api",
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256",
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = $"{_options.BaseUrl}/client-callback-popup.html"
        };
        
        var uriBuilder = new UriBuilder($"{_options.BaseUrl}/identity-server/connect/authorize");
        var query = HttpUtility.ParseQueryString(string.Empty);
        foreach (var param in authorizeParams)
        {
            query[param.Key] = param.Value;
        }
        uriBuilder.Query = query.ToString();
        
        return uriBuilder.Uri;
    }

    private async Task<Uri> GetInitialLoginRedirectUrl(Uri authorizeUri)
    {
        var authorizeResponse = await _httpClient.GetAsync(authorizeUri, HttpCompletionOption.ResponseHeadersRead);
        var loginUrl = new Uri(authorizeResponse.Headers.Location?.ToString()
                               ?? throw new Exception("Login URL not found in redirect"));

        return loginUrl;
    }
    
    private async Task<string> GetAntiForgeryTokenAsync(Uri loginUri)
    {
        var response = await _httpClient.GetAsync(loginUri);
        var content = await response.Content.ReadAsStringAsync();
       
        var match = Regex.Match(content, @"<input[^>]*name=""__RequestVerificationToken""[^>]*value=""([^""]*)",
            RegexOptions.IgnoreCase);
       
        return match.Success ? match.Groups[1].Value : throw new Exception("Antiforgery token not found");
    }

    private async Task<Uri> PostLoginDataAndGetRedirectUri(Uri loginUri, string antiforgeryToken, Uri authorizeUri)
    {
        var loginParams = new Dictionary<string, string>
        {
            ["email"] = _options.Email,
            ["password"] = _options.Password,
            ["__RequestVerificationToken"] = antiforgeryToken
        };

        var loginReturnUrl =
            $"{_options.BaseUrl}/identity-server/account/login?returnurl={Uri.EscapeDataString(authorizeUri.ToString())}";
        var loginResponse = await _httpClient.PostAsync(loginReturnUrl, new FormUrlEncodedContent(loginParams));

        var location = new Uri(loginResponse.Headers.Location?.ToString()
                               ?? throw new Exception("Authorization code not found in redirect"));

        return location;
    }

    private async Task<string> GetCodeFromLoginRedirectUri(Uri afterLoginLocationUri)
    {
        var redirectResponse = await _httpClient.GetAsync(afterLoginLocationUri);
        var location = redirectResponse.Headers.Location?.ToString() 
                   ?? throw new Exception("Authorization code not found in redirect");

        var qsCollection = HttpUtility.ParseQueryString(new Uri(location).Query);

        if (!qsCollection.HasKeys() || !qsCollection.AllKeys.Contains("code"))
        {
            throw new Exception("code not found in after login redirect");
        }

        return qsCollection.Get("code")!;
    }

    private async Task<CredentialTokens> GetTokensFromCode(string code, string codeVerifier)
    {
        var tokenParams = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["code_verifier"] = codeVerifier,
            ["redirect_uri"] = $"{_options.BaseUrl}/client-callback-popup.html"
        };

        var tokenResponse = await _httpClient.PostAsync(
            $"{_options.BaseUrl}/identity-server/connect/token",
            new FormUrlEncodedContent(tokenParams)
        );

        var result = JsonSerializer.Deserialize<CredentialTokens>(await tokenResponse.Content.ReadAsStringAsync())
                     ?? throw new Exception("Failed to extract tokens from response.");
        
        return result;
    }

    private async Task<CredentialTokens> GetTokensFromApi()
    {
        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        
        var authorizeUri = GetAuthorizeUri(codeChallenge);
        var loginUri = await GetInitialLoginRedirectUrl(authorizeUri);
        var antiforgeryToken = await GetAntiForgeryTokenAsync(loginUri);
        var afterLoginLocationUri = await PostLoginDataAndGetRedirectUri(loginUri, antiforgeryToken, authorizeUri);
        var code = await GetCodeFromLoginRedirectUri(afterLoginLocationUri);
        var result = await GetTokensFromCode(code, codeVerifier);

        return result;
    }

    private CredentialTokens? TryGetTokensFromCache() =>
        cache.TryGetValue("SquidexAccessTokens", out CredentialTokens result)
            ? result
            : null;

    private void SetTokensToCache(CredentialTokens credentialTokens)
    {
        cache.Set("SquidexAccessTokens", credentialTokens, TimeSpan.FromDays(3));
    }
}