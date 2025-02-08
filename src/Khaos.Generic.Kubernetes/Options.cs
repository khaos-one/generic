using k8s;

namespace Khaos.Generic.Kubernetes;

public sealed record Options
{
    public IReadOnlyDictionary<string, KubernetesClientConfiguration> Clusters { get; init; } 
        = default!;
}