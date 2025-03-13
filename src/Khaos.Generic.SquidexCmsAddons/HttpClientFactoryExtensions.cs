namespace Khaos.Generic.SquidexCmsAddons;

public static class HttpClientFactoryExtensions
{
    public static HttpClient CreateSquidexHttpClient(this IHttpClientFactory httpClientFactory) =>
        httpClientFactory.CreateClient(Constants.HttpClientName);

    internal static HttpClient CreateSquidexTokenResolverHttpClient(this IHttpClientFactory httpClientFactory) =>
        httpClientFactory.CreateClient(Constants.TokenResolverHttpClientName);
}
