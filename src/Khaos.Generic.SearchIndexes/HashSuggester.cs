using System.Collections.Immutable;

namespace Khaos.Generic.SearchIndexes;

public class HashSuggester<TK, TV>
    where TK: IEquatable<TK>
    where TV: IEquatable<TV>
{
    private readonly Dictionary<TK, HashSet<TV>> _map = new();
    private readonly Dictionary<TV, HashSet<TK>> _reverseMap = new();

    public void Add(IEnumerable<TK> features, TV value)
    {
        foreach (var feature in features)
        {
            AddToMap(_map, feature, value);
            AddToMap(_reverseMap, value, feature);
        }
    }

    public bool TryRemove(TV value)
    {
        if (!_reverseMap.TryGetValue(value, out var features))
        {
            return false;
        }
        
        foreach (var feature in features)
        {
            _map[feature].Remove(value);

            if (_map[feature].Count == 0)
            {
                _map.Remove(feature);
            }
        }

        _reverseMap.Remove(value);
            
        return true;
    }

    public IReadOnlyCollection<TV> Suggest(IReadOnlyCollection<TK> features)
    {
        if (features.Count == 0)
        {
            return ImmutableArray<TV>.Empty;
        }

        var suggested = default(HashSet<TV>);

        foreach (var feature in features)
        {
            if (_map.TryGetValue(feature, out var suggestedElement))
            {
                if (suggested == default)
                {
                    suggested = suggestedElement.ToHashSet();
                }
                else
                {
                    suggested.IntersectWith(suggestedElement);
                }
            }
        }

        return (IReadOnlyCollection<TV>?) suggested ?? ImmutableArray<TV>.Empty;
    }

    private static void AddToMap<T1, T2>(Dictionary<T1, HashSet<T2>> map, T1 key, T2 value) 
        where T1 : IEquatable<T1>
    {
        if (map.ContainsKey(key))
        {
            map[key].Add(value);
        }
        else
        {
            map.Add(key, new HashSet<T2> {value});
        }
    }
}