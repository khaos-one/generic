using System.Text.Json.Serialization;

namespace Khaos.Generic.Kubernetes.CustomResources.K8sNginxOrg.V1;

public sealed record Route
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = default!;
    
    [JsonPropertyName("action")]
    public RouteAction? Action { get; set; }
    
    [JsonPropertyName("route")]
    public string? RouteProperty { get; set; }
}

public sealed record RouteAction
{
    [JsonPropertyName("pass")]
    public string? Pass { get; init; }
    
    [JsonPropertyName("redirect")]
    public RouteActionRedirect? Redirect { get; init; }

    [JsonPropertyName("proxy")]
    public RouteActionProxy? Proxy { get; init; }
}

public sealed record RouteActionRedirect
{
    [JsonPropertyName("code")]
    public uint Code { get; set; }

    [JsonPropertyName("url")]
    public string Url { get; set; } = default!;
}

public sealed record RouteActionProxy
{
    [JsonPropertyName("upstream")]
    public string Upstream { get; init; } = default!;

    [JsonPropertyName("requestHeaders")]
    public RequestHeaders? RequestHeaders { get; init; }
}

public sealed record RequestHeaders
{
    [JsonPropertyName("pass")]
    public bool? Pass { get; init; }

    [JsonPropertyName("set")]
    public ICollection<RequestHeaderConfig>? Set { get; init; }
}

public sealed record RequestHeaderConfig
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("value")]
    public string Value { get; init; } = default!;
}

public sealed record PolicyReference
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = default!;

    [JsonPropertyName("namespace")]
    public string Namespace { get; init; } = default!;
}