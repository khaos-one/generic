namespace Khaos.Generic.Trees;

public record FlatTreeNode<TK, TV>(TK Key, TK ParentKey, TV? Value = default)
    where TK : IEquatable<TK>;