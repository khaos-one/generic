using FluentAssertions;
using Khaos.Generic.SquidexCmsAddons.Auth;
using Microsoft.Extensions.Caching.Memory;

namespace Khaos.Generic.SquidexCmsAddons.Tests.Auth;

public sealed class CredentialTokensRetrieverShould
{
    private static CredentialTokensRetriever Create() => new(
        new Microsoft.Extensions.Options.OptionsWrapper<Options>(
            new Options
            {
                BaseUrl = "(redacted)",
                ClientId = "(redacted)",
                Email = "(redacted)",
                Password = "(redacted)"
            }), 
            new TestHttpClientFactory(new HttpClient(new HttpClientHandler {AllowAutoRedirect = false})),
        new MemoryCache(new MemoryCacheOptions()));
    
    [Fact]
    public async Task RetrieveCredentialTokensAsync()
    {
        var sut = Create();
        var tokens = await sut.GetCredentialTokensAsync();
        
        tokens.AccessToken.Should().NotBeEmpty();
    }
}