using FluentAssertions;
using Xunit;

namespace Khaos.Generic.SearchIndexes.Tests;

public sealed class HashSuggesterShould
{
    [Fact]
    public void SuggestElementsBasedOnIntersectingFeaturesSynthetic1()
    {
        var sut = new HashSuggester<string, string>();

        sut.Add(new[] {"feature 1", "feature 2"}, "first element");
        sut.Add(new [] {"feature 3", "feature 4"}, "second element");

        var result = sut.Suggest(new[] {"feature 1", "feature 2", "feature 6"});

        result.Should().BeEquivalentTo(new[] {"first element"});
    }
}