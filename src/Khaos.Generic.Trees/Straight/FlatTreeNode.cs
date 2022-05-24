namespace Khaos.Generic.Trees.Straight;

public record FlatTreeNode<TK, TV>(TK Key, TK? ParentKey = default, TV? Value = default)
    where TK : IEquatable<TK>;