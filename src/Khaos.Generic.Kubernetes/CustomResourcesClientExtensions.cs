using k8s;
using k8s.Models;

namespace Khaos.Generic.Kubernetes;

public static class CustomResourcesClientExtensions
{
    public static async Task<TResource?> GetAsync<TResource>(
        this ICustomObjectsOperations customObjectsOperations,
        string name, string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await customObjectsOperations
                .GetNamespacedCustomObjectAsync<TResource>(
                    group, version, @namespace, plural, name,
                    cancellationToken);

        return result;
    }

    public static async Task<TResource?> PatchAsync<TResource>(
        this ICustomObjectsOperations customObjectsOperations,
        TResource resource,
        string name,
        string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await customObjectsOperations
                .PatchNamespacedCustomObjectAsync<TResource>(
                    new V1Patch(resource), 
                    group, version, @namespace, plural, name,
                    cancellationToken: cancellationToken);

        return result;
    }

    public static async Task<TResource?> DeleteAsync<TResource>(
        this ICustomObjectsOperations customObjectsOperations,
        string name,
        string @namespace = "default",
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await customObjectsOperations
                .DeleteNamespacedCustomObjectAsync<TResource>(
                    group, version, @namespace, plural, name,
                    cancellationToken: cancellationToken);

        return result;
    }

    public static async Task<IList<TResource>> ListAsync<TResource>(
        this ICustomObjectsOperations customObjectsOperations,
        string @namespace = "default",
        string? fieldSelector = null,
        string? labelSelector = null,
        CancellationToken cancellationToken = default)
        where TResource : class, IKubernetesObject, new()
    {
        var (group, version, plural) = GetResourceInfo<TResource>();
        var result =
            await customObjectsOperations
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