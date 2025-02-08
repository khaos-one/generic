using System.Text.Json.Serialization;

namespace Khaos.Generic.Kubernetes.CustomResources.K8sNginxOrg.V1;

public sealed class VirtualServerRoute : KubernetesResource<VirtualServerSpec>
{
    public override string ApiVersion { get; set; } = "k8s.nginx.org/v1";
    public override string Kind { get; set; } = "VirtualServerRoute";
}

public sealed record VirtualServerRouteSpec
{
    [JsonPropertyName("host")]
    public string Host { get; set; } = default!;

    [JsonPropertyName("routes")]
    public ICollection<Route> Routes { get; set; } = new List<Route>();

    [JsonPropertyName("upstreams")]
    public ICollection<Upstream> Upstreams { get; set; } = new List<Upstream>();
}