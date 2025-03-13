namespace Khaos.Generic.SquidexCmsAddons.Auth;

internal sealed class AuthenticatedHandler(ICredentialTokensRetriever credentialTokensRetriever) 
    : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var tokens = await credentialTokensRetriever.GetCredentialTokensAsync(cancellationToken);
        var accessToken = tokens.AccessToken;
        request.Headers.Authorization = new("Bearer", accessToken);

        return await base.SendAsync(request, cancellationToken);
    }
}
