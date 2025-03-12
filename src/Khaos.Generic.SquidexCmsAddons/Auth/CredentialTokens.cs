namespace Khaos.Generic.SquidexCmsAddons.Auth;

public record CredentialTokens
{
    [System.Text.Json.Serialization.JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = null!;

    [System.Text.Json.Serialization.JsonPropertyName("token_type")]
    public string TokenType { get; init; } = null!;

    [System.Text.Json.Serialization.JsonPropertyName("expires_in")]
    [System.Text.Json.Serialization.JsonConverter(typeof(ExpiresAtConverter))]
    public DateTime ExpiresAt { get; init; }

    [System.Text.Json.Serialization.JsonPropertyName("scope")]
    public string Scope { get; init; } = null!;
}

internal sealed class ExpiresAtConverter : System.Text.Json.Serialization.JsonConverter<DateTime>
{
    public override DateTime Read(
        ref System.Text.Json.Utf8JsonReader reader, 
        Type typeToConvert,
        System.Text.Json.JsonSerializerOptions options)
    {
        var seconds = reader.GetInt64();
        return DateTime.UtcNow.AddSeconds(seconds);
    }

    public override void Write(
        System.Text.Json.Utf8JsonWriter writer, 
        DateTime value,
        System.Text.Json.JsonSerializerOptions options)
    {
        throw new NotSupportedException("Serialization not required in this context.");
    }
}