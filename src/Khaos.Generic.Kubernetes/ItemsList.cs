using k8s;
using k8s.Models;

namespace Khaos.Generic.Kubernetes;

internal class ItemsList<TResource> 
    : IKubernetesObject<V1ListMeta>, IItems<TResource>
{
    public required string ApiVersion { get; set; }
    public required string Kind { get; set; }
    public required V1ListMeta Metadata { get; set; }
    public required IList<TResource> Items { get; set; }
}