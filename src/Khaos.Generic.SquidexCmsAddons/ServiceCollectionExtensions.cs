using Microsoft.Extensions.DependencyInjection;
using Khaos.Generic.SquidexCmsAddons.Auth;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Configuration;

namespace Khaos.Generic.SquidexCmsAddons;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds an authenticated HTTP handler to the service collection and registers 
    /// other required machinery.
    /// </summary>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <param name="configureOptions">An action to configure the authentication options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSquidexAuthenticatedHttpHandler(
        this IServiceCollection services, 
        Action<Auth.Options> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddMemoryCache();
        services.AddScoped<ICredentialTokensRetriever, CredentialTokensRetriever>();
        services.AddScoped<AuthenticatedHandler>();

        services.AddHttpClient(Constants.TokenResolverHttpClientName, (sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<Auth.Options>>();
            client.BaseAddress = new Uri(options.Value.BaseUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        services
            .AddHttpClient(Constants.HttpClientName, ConfigureHttpClient)
            .AddHttpMessageHandler<AuthenticatedHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        return services;
    }

    /// <summary>
    /// Adds an authenticated HTTP handler to the service collection and registers 
    /// other required machinery.
    /// </summary>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <param name="configureOptions">An action to configure the authentication options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSquidexAuthenticatedHttpHandler(
        this IServiceCollection services,
        IConfiguration configuration) =>
        AddSquidexAuthenticatedHttpHandler(services, configuration.Bind);

    private static void ConfigureHttpClient(IServiceProvider sp, HttpClient client)
    {
        var options = sp.GetRequiredService<IOptions<Auth.Options>>();
        client.BaseAddress = new Uri(options.Value.BaseUrl);
        client.DefaultRequestHeaders.Add("X-Flatten", "1");
    }
}
