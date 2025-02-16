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
    
    [JsonPropertyName("return")]
    public RouteActionReturn? Return { get; init; }
}

public sealed record RouteActionReturn
{
    [JsonPropertyName("code")]
    public ushort Code { get; init; }
    
    [JsonPropertyName("type")]
    public string Type { get; init; }
    
    [JsonPropertyName("body")]
    public string Body { get; init; }
    
    [JsonPropertyName("headers")]
    public ICollection<HeaderConfig>? Headers { get; init; }
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

    [JsonPropertyName("rewritePath")]
    public string RewritePath { get; init; } = default!;

    [JsonPropertyName("requestHeaders")]
    public RequestHeaders? RequestHeaders { get; init; }
    
    [JsonPropertyName("responseHeaders")]
    public ResponseHeaders? ResponseHeaders { get; init; }
}

public sealed record RequestHeaders
{
    [JsonPropertyName("pass")]
    public bool? Pass { get; init; }

    [JsonPropertyName("set")]
    public ICollection<HeaderConfig>? Set { get; init; }
    
    [JsonPropertyName("add")]
    public ICollection<HeaderConfig>? Add { get; init; }
}

public sealed record ResponseHeaders
{
    public ICollection<HeaderConfig>? Add { get; init; }
}

public sealed record HeaderConfig
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