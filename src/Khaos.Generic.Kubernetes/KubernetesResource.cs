using System.Text.Json.Serialization;
using k8s;
using k8s.Models;

namespace Khaos.Generic.Kubernetes;

public interface IKubernetesResource : IKubernetesObject;

public interface IKubernetesResource<TMeta, TSpec, TStatus> 
    : IKubernetesObject<TMeta>, IKubernetesResource
    where TStatus : class
{
    [JsonPropertyName("spec")]
    TSpec Spec { get; set; }
    [JsonPropertyName("status")]
    TStatus Status { get; set; }
}

public abstract class KubernetesResource<TMeta, TSpec, TStatus> 
    : IKubernetesResource<TMeta, TSpec, TStatus> 
    where TStatus : class
{
    [JsonPropertyName("apiVersion")]
    public abstract string ApiVersion { get; set; }
    [JsonPropertyName("kind")]
    public abstract string Kind { get; set; }
    [JsonPropertyName("metadata")]
    public required TMeta Metadata { get; set; }
    public required TSpec Spec { get; set; }
    public required TStatus Status { get; set; }
}

public interface IKubernetesResource<TMeta, TSpec> 
    : IKubernetesResource<TMeta, TSpec, V1Status>;

public abstract class KubernetesResource<TMeta, TSpec> 
    : IKubernetesResource<TMeta, TSpec>
{
    [JsonPropertyName("apiVersion")]
    public abstract string ApiVersion { get; set; }
    [JsonPropertyName("kind")]
    public abstract string Kind { get; set; }
    [JsonPropertyName("metadata")]
    public required TMeta Metadata { get; set; }
    public required TSpec Spec { get; set; }
    public required V1Status Status { get; set; }
}

public interface IKubernetesResource<TSpec> 
    : IKubernetesResource<V1ObjectMeta, TSpec>;

public abstract class KubernetesResource<TSpec> 
    : IKubernetesResource<TSpec>
{
    [JsonPropertyName("apiVersion")]
    public abstract string ApiVersion { get; set; }
    [JsonPropertyName("kind")]
    public abstract string Kind { get; set; }
    [JsonPropertyName("metadata")]
    public required V1ObjectMeta Metadata { get; set; }

    [JsonPropertyName("spec")]
    public required TSpec Spec { get; set; }
    [JsonPropertyName("status")]
    public required V1Status Status { get; set; }
}