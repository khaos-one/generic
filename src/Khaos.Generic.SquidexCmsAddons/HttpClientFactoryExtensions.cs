namespace Khaos.Generic.SquidexCmsAddons;

public static class HttpClientFactoryExtensions
{
    public static HttpClient CreateSquidexHttpClient(this IHttpClientFactory httpClientFactory) =>
        httpClientFactory.CreateClient(Constants.HttpClientName);
}
