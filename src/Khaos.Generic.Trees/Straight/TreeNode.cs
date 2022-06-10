using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace Khaos.Generic.Trees.Straight;

public class TreeNode<TK, TV> 
    where TK : IEquatable<TK>
{
    public TK Key { get; }
    public TV? Value { get; set; }

    private readonly ObservableCollection<TreeNode<TK, TV>> _children = new();
    private readonly HashSet<TK> _keysLookup = new();
    private readonly TreeNode<TK, TV>? _parent;

    private NotifyCollectionChangedEventHandler? _parentChildrenOnCollectionChanged;
    
    public TreeNode(TK key, TV? value = default)
        : this(key, value, default)
    { }

    protected TreeNode(TK key, TV? value = default, TreeNode<TK, TV>? parent = default)
    {
        Key = key;
        Value = value;

        _parent = parent;
        _keysLookup.Add(key);
        _children.CollectionChanged += ChildrenOnCollectionChanged;

        if (_parent is not null)
        {
            _parentChildrenOnCollectionChanged += _parent.ChildrenOnCollectionChanged;
        }
    }

    protected TreeNode(FlatTreeNode<TK, TV> flatNode, TreeNode<TK, TV> parent)
        : this(flatNode.Key, flatNode.Value, parent)
    { }

    protected TreeNode(TreeNode<TK, TV> original, TreeNode<TK, TV>? newParent = default)
        : this(original.Key, original.Value, newParent)
    {
        _children = original._children;
        
        VisitBackward(child => child.TapChildrenOnCollectionChangedWithAllItems());
    }

    public IReadOnlyCollection<TreeNode<TK, TV>> Children => _children.ToImmutableArray();
    public int DirectChildrenCount => _children.Count;
    public bool HasChildren => _children.Count > 0;

    public IEnumerable<TR> SelectForward<TR>(Func<TreeNode<TK, TV>, TR> fn)
    {
        yield return fn(this);

        foreach (var child in _children)
        {
            foreach (var result in child.SelectForward(fn))
            {
                yield return result;
            }
        }
    }

    public IEnumerable<TR> SelectBackward<TR>(Func<TreeNode<TK, TV>, TR> fn)
    {
        foreach (var child in _children)
        {
            foreach (var result in child.SelectBackward(fn))
            {
                yield return result;
            }
        }
        
        yield return fn(this);
    }
    
    public void VisitForward(Action<TreeNode<TK, TV>> fn)
    {
        fn(this);

        foreach (var child in _children)
        {
            child.VisitForward(fn);
        }
    }

    public void VisitBackward(Action<TreeNode<TK, TV>> fn)
    {
        foreach (var child in _children)
        {
            child.VisitBackward(fn);
        }
        
        fn(this);
    }

    public TR? FirstForwardOrDefault<TR>(
        Func<TreeNode<TK, TV>, bool> pathCriteria,
        Func<TreeNode<TK, TV>, bool> nodeCriteria, 
        Func<TreeNode<TK, TV>, TR> fn, 
        TR? @default = default)
    {
        if (!pathCriteria(this))
        {
            return @default;
        }

        if (nodeCriteria(this))
        {
            return fn(this);
        }

        foreach (var child in _children)
        {
            var result = child.FirstForwardOrDefault(pathCriteria, nodeCriteria, fn, @default);

            if (!Equals(result, @default))
            {
                return result;
            }
        }

        return @default;
    }
    
    public TR? FirstBackwardOrDefault<TR>(
        Func<TreeNode<TK, TV>, bool> pathCriteria,
        Func<TreeNode<TK, TV>, bool> nodeCriteria, 
        Func<TreeNode<TK, TV>, TR> fn, 
        TR? @default = default)
    {
        if (!pathCriteria(this))
        {
            return @default;
        }

        foreach (var child in _children)
        {
            var result = child.FirstForwardOrDefault(pathCriteria, nodeCriteria, fn, @default);

            if (!Equals(result, @default))
            {
                return result;
            }
        }
        
        if (nodeCriteria(this))
        {
            return fn(this);
        }

        return @default;
    }

    public TreeNode<TK, TV>? FindByKeyOrDefault(TK key) =>
        FirstForwardOrDefault(
            treeNode => treeNode._keysLookup.Contains(key),
            treeNode => Equals(treeNode.Key, key),
            treeNode => treeNode);

    public void Add(TreeNode<TK, TV> node)
    {
        _children.Add(new TreeNode<TK, TV>(node, this));
    }

    public void Add(FlatTreeNode<TK, TV> node)
    {
        _children.Add(new TreeNode<TK, TV>(node, this));
    }

    public bool TryAdd(FlatTreeNode<TK, TV> node) =>
        FirstForwardOrDefault(
            treeNode => treeNode._keysLookup.Contains(node.ParentKey),
            treeNode => Equals(treeNode.Key, node.ParentKey),
            treeNode =>
            {
                treeNode.Add(node);
                
                return true;
            });

    public void Clear() =>
        VisitBackward(node =>
        {
            if (node._children.Any())
            {
                foreach (var child in node._children.ToImmutableArray())
                {
                    child._parentChildrenOnCollectionChanged = null;
                    node._children.Remove(child);
                }
            }
        });

    public bool TryRemove(TK key)
    {
        var node = FindByKeyOrDefault(key);

        if (node is null)
        {
            return false;
        }

        node.Clear();
        
        node._parentChildrenOnCollectionChanged = null;
        node._parent?._children.Remove(node);

        return true;

    }

    public bool TryCut(TK key, out TreeNode<TK, TV>? node)
    {
        node = FindByKeyOrDefault(key);

        if (node is null)
        {
            return false;
        }

        node._parentChildrenOnCollectionChanged = null;
        node._parent?._children.Remove(node);

        return true;
    }

    public bool TryMove(TK key, TK newParentKey)
    {
        var newParentNode = FindByKeyOrDefault(newParentKey);

        if (newParentNode is null)
        {
            return false;
        }

        if (!TryCut(key, out var targetNode))
        {
            return false;
        }

        targetNode!._parentChildrenOnCollectionChanged += newParentNode.ChildrenOnCollectionChanged;
        newParentNode._children.Add(targetNode);

        targetNode.VisitBackward(node => node.TapChildrenOnCollectionChangedWithAllItems());

        return true;
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                foreach (TreeNode<TK, TV> node in e.NewItems ?? Array.Empty<TreeNode<TK, TV>>())
                {
                    _keysLookup.Add(node.Key);
                }

                break;
            
            case NotifyCollectionChangedAction.Remove:
                foreach (TreeNode<TK, TV> node in e.OldItems ?? Array.Empty<TreeNode<TK, TV>>())
                {
                    _keysLookup.Remove(node.Key);
                }

                break;
        }

        _parentChildrenOnCollectionChanged?.Invoke(sender, e);
    }

    private void TapChildrenOnCollectionChangedWithAllItems()
    {
        var @event = new NotifyCollectionChangedEventArgs(
            NotifyCollectionChangedAction.Add,
            _children.ToList());

        ChildrenOnCollectionChanged(_children, @event);
    }
}