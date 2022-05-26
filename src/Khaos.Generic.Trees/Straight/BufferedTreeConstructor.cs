using System.Collections.Immutable;

namespace Khaos.Generic.Trees.Straight;

public class BufferedTreeConstructor<TK, TV>
    where TK : IEquatable<TK>
{
    private readonly List<FlatTreeNode<TK, TV>> _buffer = new();
    private readonly TreeNode<TK, TV> _root;

    public BufferedTreeConstructor(TreeNode<TK, TV> root)
    {
        _root = root;
    }

    public int BufferCount => _buffer.Count;
    public bool IsEmpty => _buffer.Count == 0;

    public IReadOnlyCollection<FlatTreeNode<TK, TV>> Buffer => _buffer.ToImmutableArray();

    public void Add(FlatTreeNode<TK, TV> flatNode)
    {
        TryAdd(flatNode);
        ProcessNodesBuffer();
    }

    private void TryAdd(FlatTreeNode<TK, TV> flatNode)
    {
        var result = _root.TryAdd(flatNode);

        if (!result)
        {
            _buffer.Add(flatNode);
        }
    }

    private void ProcessNodesBuffer()
    {
        var bufferCopy = _buffer.ToImmutableArray();

        foreach (var node in bufferCopy)
        {
            _buffer.Remove(node);
            TryAdd(node);
        }
    }
}