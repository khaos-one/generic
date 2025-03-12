namespace Khaos.Generic.SquidexCmsAddons.Auth;

public record class Options
{
    public string BaseUrl { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public string ClientId { get; init; } = "squidex-frontend";
}
