using System.Text.Json.Serialization;

namespace Khaos.Generic.Kubernetes.CustomResources.CertManagerIo.V1;

public sealed class Certificate : KubernetesResource<CertificateSpec>
{
    public override string ApiVersion { get; set; } = "cert-manager.io/v1";
    public override string Kind { get; set; } = "Certificate";
}

public sealed class CertificateSpec
{
    [JsonPropertyName("dnsNames")]
    public ICollection<string> DnsNames { get; set; } = new List<string>();
    
    public IssuerRefSpec IssuerRef { get; set; } = default!;

    [JsonPropertyName("secretName")]
    public string SecretName { get; set; } = default!;
}

public sealed record IssuerRefSpec
{
    public static readonly IssuerRefSpec LetsEncrypt = new()
    {
        Group = "cert-manager.io",
        Kind = "ClusterIssuer",
        Name = "letsencrypt"
    };
    
    [JsonPropertyName("group")]
    public string Group { get; set; } = default!;
    
    [JsonPropertyName("kind")]
    public string Kind { get; set; } = default!;
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = default!;
}