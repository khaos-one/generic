using System.Text.Json.Serialization;

namespace Khaos.Generic.Kubernetes.CustomResources.K8sNginxOrg.V1;

public sealed class VirtualServer : KubernetesResource<VirtualServerSpec>
{
    public override string ApiVersion { get; set; } = "k8s.nginx.org/v1";
    public override string Kind { get; set; } = "VirtualServer";
}

public sealed record VirtualServerSpec 
{
    [JsonPropertyName("host")]
    public string Host { get; init; } = default!;
    
    [JsonPropertyName("tls")]
    public TlsSpec? Tls { get; init; }
    
    [JsonPropertyName("gunzip")]
    public bool GUnZip { get; init; }

    [JsonPropertyName("upstreams")] 
    public ICollection<Upstream> Upstreams { get; init; } = new List<Upstream>();

    [JsonPropertyName("routes")]
    public ICollection<Route> Routes { get; init; } = new List<Route>();
    
    [JsonPropertyName("server-snippets")]
    public string? ServerSnippets { get; init; }
    
    [JsonPropertyName("http-snippets")]
    public string? HttpSnippets { get; init; }

    [JsonPropertyName("policies")]
    public ICollection<PolicyReference>? Policies { get; init; }
}

public sealed record TlsCertManagerSpec
{
    [JsonPropertyName("cluster-issuer")]
    public string ClusterIssuer { get; set; } = default!;
}

public sealed record TlsRedirectSpec
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = default!;
}

public sealed record TlsSpec
{
    [JsonPropertyName("secret")]
    public string Secret { get; set; } = default!;

    [JsonPropertyName("cert-manager")]
    public TlsCertManagerSpec CertManager { get; set; } = default!;

    [JsonPropertyName("redirect")]
    public TlsRedirectSpec Redirect { get; set; } = default!;
}