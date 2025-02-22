using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Khaos.Generic.Kubernetes;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKubernetesCustomResourcesClient(
        this IServiceCollection services, Action<Options> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IClientRegistry, ClientRegistry>();
        return services;
    }

    public static IServiceCollection AddKubernetesCustomResourcesClient(
        this IServiceCollection services, IConfiguration configuration)
        => AddKubernetesCustomResourcesClient(services, configuration.Bind);
}