using System.Collections.Concurrent;
using System.Collections.Immutable;
using ConcurrentCollections;

namespace Khaos.Generic.SearchIndexes;

public class ConcurrentHashSuggester<TK, TV>
    where TK : IEquatable<TK>
    where TV : IEquatable<TV>
{
    private readonly ConcurrentDictionary<TK, ConcurrentHashSet<TV>> _map = new();
    private readonly ConcurrentDictionary<TV, ConcurrentHashSet<TK>> _reverseMap = new();

    private readonly object _writeLock = new();

    public void Add(IReadOnlyCollection<TK> features, TV value)
    {
        lock (_writeLock)
        {
            foreach (var feature in features)
            {
                _map.AddOrUpdate(feature,
                    new ConcurrentHashSet<TV> {value},
                    (_, currentValue) =>
                    {
                        currentValue.Add(value);
                        return currentValue;
                    });

                _reverseMap.AddOrUpdate(value,
                    new ConcurrentHashSet<TK> {feature},
                    (_, currentValue) =>
                    {
                        currentValue.Add(feature);
                        return currentValue;
                    });
            }
        }
    }

    public bool TryRemove(TV value)
    {
        lock (_writeLock)
        {
            if (!_reverseMap.TryGetValue(value, out var features))
            {
                return false;
            }

            foreach (var feature in features)
            {
                _map[feature].TryRemove(value);

                if (_map[feature].Count == 0)
                {
                    _map.TryRemove(feature, out _);
                }
            }

            _reverseMap.TryRemove(value, out _);
        }
        
        return true;
    }

    public IReadOnlyCollection<TV> Suggest(IReadOnlyCollection<TK> features, bool onlyIntersecting = false)
    {
        if (features.Count == 0)
        {
            return ImmutableArray<TV>.Empty;
        }
        
        var suggested = default(HashSet<TV>);

        foreach (var feature in features)
        {
            // ReSharper disable once InconsistentlySynchronizedField
            if (_map.TryGetValue(feature, out var suggestedElement))
            {
                if (suggested == default)
                {
                    suggested = suggestedElement.ToHashSet();
                }
                else
                {
                    if (onlyIntersecting)
                    {
                        suggested.IntersectWith(suggestedElement);
                    }
                    else
                    {
                        suggested.UnionWith(suggestedElement);
                    }
                }
            }
        }

        return (IReadOnlyCollection<TV>?) suggested ?? ImmutableArray<TV>.Empty;
    }
}