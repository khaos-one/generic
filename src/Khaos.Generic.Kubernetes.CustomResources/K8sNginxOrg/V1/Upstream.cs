using System.Text.Json.Serialization;

namespace Khaos.Generic.Kubernetes.CustomResources.K8sNginxOrg.V1;

public sealed record Upstream
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
    
    [JsonPropertyName("port")]
    public uint Port { get; set; }
    
    [JsonPropertyName("service")]
    public string Service { get; set; } = default!;
    
    [JsonPropertyName("client-max-body-size")]
    public string? ClientMaxBodySize { get; set; }
}