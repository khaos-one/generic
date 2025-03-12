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

        services.AddHttpClient<CredentialTokensRetriever>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<Auth.Options>>();
            client.BaseAddress = new Uri(options.Value.BaseUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        services.AddTransient<ICredentialTokensRetriever, CredentialTokensRetriever>();
        services.AddTransient<AuthenticatedHandler>();

        services.AddMemoryCache();

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

    /// <summary>
    /// Adds an implementation of <see cref="HttpClient"/> to the service collection. Use it
    /// instead of a regular <see cref="HttpClient"/> to benefit from the authenticated handler.
    /// </summary>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <param name="clientName">The name of the client to add.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSquidexHttpClient<TClient>(this IServiceCollection services)
        where TClient : class
    {
        services
            .AddHttpClient<TClient>(ConfigureHttpClient)
            .AddHttpMessageHandler<AuthenticatedHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        return services;
    }

    /// <summary>
    /// Adds an implementation of <see cref="HttpClient"/> to the service collection. Use it
    /// instead of a regular <see cref="HttpClient"/> to benefit from the authenticated handler.
    /// </summary>
    /// <param name="services">The service collection to add the handler to.</param>
    /// <param name="clientName">The name of the client to add.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSquidexHttpClient(this IServiceCollection services, string clientName)
    {
        services
            .AddHttpClient(clientName, ConfigureHttpClient)
            .AddHttpMessageHandler<AuthenticatedHandler>()
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false });

        return services;
    }
    private static void ConfigureHttpClient(IServiceProvider sp, HttpClient client)
    {
        var options = sp.GetRequiredService<IOptions<Auth.Options>>();
        client.BaseAddress = new Uri(options.Value.BaseUrl);
        client.DefaultRequestHeaders.Add("X-Flatten", "1"); 
    }
}
