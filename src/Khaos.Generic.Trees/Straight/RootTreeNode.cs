using System.Collections.Immutable;

namespace Khaos.Generic.Trees.Straight;

public record RootTreeNode<TK, TV>(TK Key, TV? Value, ICollection<TreeNode<TK, TV>> Children)
    : TreeNode<TK, TV>(Key, Value, Children)
    where TK : IEquatable<TK>
{
    private readonly List<FlatTreeNode<TK, TV>> _nodesBuffer = new();

    public RootTreeNode(TK key, TV? value = default)
        : this(key, value, new List<TreeNode<TK, TV>>())
    { }
    
    public RootTreeNode()
        : this(default!, default)
    { }
    
    public void Append(FlatTreeNode<TK, TV> flatNode)
    {
        TryAppendInternal(flatNode);
        ProcessNodesBuffer();
    }

    private void TryAppendInternal(FlatTreeNode<TK, TV> flatNode)
    {
        var result = TryAppend(flatNode);

        if (!result)
        {
            _nodesBuffer.Add(flatNode);
        }
    }

    private void ProcessNodesBuffer()
    {
        var bufferCopy = _nodesBuffer.ToImmutableArray();

        foreach (var node in bufferCopy)
        {
            _nodesBuffer.Remove(node);
            TryAppendInternal(node);
        }
    }
}