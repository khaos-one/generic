using System.Collections.Generic;
using FluentAssertions;
using Xunit;

namespace Khaos.Generic.SearchIndexes.Tests;

public sealed class ConcurrentHashSuggesterShould
{
    [Fact]
    public void SuggestElementsBasedOnIntersectingFeatures1()
    {
        var sut = new ConcurrentHashSuggester<string, string>();

        sut.Add(new[] {"feature 1", "feature 2"}, "first element");
        sut.Add(new [] {"feature 3", "feature 4"}, "second element");

        var result = sut.Suggest(new[] {"feature 1", "feature 2"});

        result.Should().BeEquivalentTo(new[] {"first element"});
    }
    
    [Fact]
    public void SuggestElementsWhenIntersecting()
    {
        var sut = new ConcurrentHashSuggester<string, string>();

        sut.Add(new[] {"feature 1", "feature 2"}, "first element");
        sut.Add(new [] {"feature 2", "feature 3"}, "second element");

        var result = sut.Suggest(new[] { "feature 2"});

        result.Should().BeEquivalentTo(new[] {"first element", "second element"});
    }
}