using Microsoft.Extensions.DependencyInjection;

namespace Khaos.Generic.Kubernetes;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKubernetes(this IServiceCollection services, Action<Options> configure)
    {
        services.Configure(configure);
        services.AddSingleton<BaseClientRegistry>();
        services.AddSingleton<ICustomResourcesClient, CustomResourcesClient>();
        return services;
    }
}