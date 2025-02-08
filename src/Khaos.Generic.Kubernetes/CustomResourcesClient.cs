using k8s;
using k8s.Models;

namespace Khaos.Generic.Kubernetes;

public interface ICustomResourcesClient
{
    Task<TResource?> GetAsync<TResource>(string name, string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new();
    
    Task<TResource?> PatchAsync<TResource>(
        TResource resource,
        string name,
        string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new();
    
    Task<TResource?> DeleteAsync<TResource>(
        string name,
        string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new();
    
    Task<IList<TResource>> ListAsync<TResource>(
        string @namespace = "default",
        string? fieldSelector = null,
        string? labelSelector = null,
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new();
}

public class CustomResourcesClient : ICustomResourcesClient
{
    private readonly IKubernetes baseClient;

    public CustomResourcesClient(IKubernetes baseClient)
    {
        this.baseClient = baseClient;
    }

    public async Task<TResource?> GetAsync<TResource>(string name, string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await baseClient.CustomObjects
                .GetNamespacedCustomObjectAsync<TResource>(
                    group, version, @namespace, plural, name,
                    cancellationToken);

        return result;
    }

    public async Task<TResource?> PatchAsync<TResource>(
        TResource resource,
        string name,
        string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await baseClient.CustomObjects
                .PatchNamespacedCustomObjectAsync<TResource>(
                    new V1Patch(resource), 
                    group, version, @namespace, plural, name,
                    cancellationToken: cancellationToken);

        return result;
    }

    public async Task<TResource?> DeleteAsync<TResource>(
        string name,
        string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await baseClient.CustomObjects
                .DeleteNamespacedCustomObjectAsync<TResource>(
                    group, version, @namespace, plural, name,
                    cancellationToken: cancellationToken);

        return result;
    }

    public async Task<IList<TResource>> ListAsync<TResource>(
        string @namespace = "default",
        string? fieldSelector = null,
        string? labelSelector = null,
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await baseClient.CustomObjects
                .ListNamespacedCustomObjectAsync<ItemsList<TResource>>(
                    group, version, @namespace, plural,
                    fieldSelector: fieldSelector,
                    labelSelector: labelSelector,
                    cancellationToken: cancellationToken);

        return result.Items;
    }

    private static (string Group, string Version, string Plural) GetResourceInfo<TResource>()
        where TResource : class, IKubernetesObject, new()
    {
        var instance = new TResource();
        var (group, version) = KubernetesResourceMetadataHelper.ParseApiVersion(instance.ApiVersion);
        var plural = KubernetesResourceMetadataHelper.GetPluralName(instance.Kind);

        return (group, version, plural);
    }
}