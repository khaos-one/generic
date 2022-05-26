using System.Linq;
using FluentAssertions;
using Khaos.Generic.Trees.Straight;
using Xunit;

namespace Khaos.Generic.Trees.Tests.Straight;

public sealed class TreeNodeShould
{
    [Fact]
    public void AddNodes()
    {
        var nodes = new[]
        {
            new FlatTreeNode<string, object>("n1", "n0"),
            new FlatTreeNode<string, object>("n2", "n1"),
            new FlatTreeNode<string, object>("n3", "n2"),
            new FlatTreeNode<string, object>("n4", "n0")
        };
        
        var sut = new TreeNode<string, object>("n0");

        foreach (var node in nodes)
        {
            var result = sut.TryAdd(node);

            result.Should().BeTrue();
        }

        sut.Children.Count.Should().Be(2);
        
        sut.Children.ElementAt(0).Key.Should().Be("n1");
        sut.Children.ElementAt(0).Children.Count.Should().Be(1);
        sut.Children.ElementAt(0).Children.ElementAt(0).Key.Should().Be("n2");
        sut.Children.ElementAt(0).Children.ElementAt(0).Children.Count.Should().Be(1);
        sut.Children.ElementAt(0).Children.ElementAt(0).Children.ElementAt(0).Key.Should().Be("n3");
        sut.Children.ElementAt(0).Children.ElementAt(0).Children.ElementAt(0).Children.Count.Should().Be(0);
        
        sut.Children.ElementAt(1).Key.Should().Be("n4");
        sut.Children.ElementAt(1).Children.Count.Should().Be(0);
    }
    
    [Fact]
    public void DoNotAddNodesWhenThereIsNoParent()
    {
        var nodes = new[]
        {
            new FlatTreeNode<string, object>("n1", "n0"),
            new FlatTreeNode<string, object>("n2", "n1"),
            new FlatTreeNode<string, object>("n3", "n4"),
            new FlatTreeNode<string, object>("n4", "n0")
        };
        
        var sut = new TreeNode<string, object>("n0");

        foreach (var node in nodes)
        {
            var result = sut.TryAdd(node);

            if (node.Key != "n3")
            {
                result.Should().BeTrue();
            }
            else
            {
                result.Should().BeFalse();
            }
        }

        sut.Children.Count.Should().Be(2);
        
        sut.Children.ElementAt(0).Key.Should().Be("n1");
        sut.Children.ElementAt(0).Children.Count.Should().Be(1);
        sut.Children.ElementAt(0).Children.ElementAt(0).Key.Should().Be("n2");
        sut.Children.ElementAt(0).Children.ElementAt(0).Children.Count.Should().Be(0);
        
        sut.Children.ElementAt(1).Key.Should().Be("n4");
        sut.Children.ElementAt(1).Children.Count.Should().Be(0);
    }
    
    [Fact]
    public void FindNodeByKey()
    {
        var nodes = new[]
        {
            new FlatTreeNode<string, object>("n1", "n0"),
            new FlatTreeNode<string, object>("n2", "n1"),
            new FlatTreeNode<string, object>("n3", "n2"),
            new FlatTreeNode<string, object>("n4", "n0")
        };
        
        var sut = new TreeNode<string, object>("n0");

        foreach (var node in nodes)
        {
            sut.TryAdd(node);
        }

        var result = sut.FindByKeyOrDefault("n2");

        result.Should().NotBeNull();
        result!.Key.Should().Be("n2");
        result.Children.Count.Should().Be(1);
    }
    
    [Fact]
    public void ClearTargetNode()
    {
        var nodes = new[]
        {
            new FlatTreeNode<string, object>("n1", "n0"),
            new FlatTreeNode<string, object>("n2", "n1"),
            new FlatTreeNode<string, object>("n3", "n2"),
            new FlatTreeNode<string, object>("n4", "n0")
        };
        
        var sut = new TreeNode<string, object>("n0");

        foreach (var node in nodes)
        {
            sut.TryAdd(node);
        }

        var nodeWillBeCleared = sut.FindByKeyOrDefault("n3");
        var result = sut.FindByKeyOrDefault("n2");

        result.Should().NotBeNull();
        result!.Key.Should().Be("n2");
        
        result.Clear();

        result.Children.Should().BeEmpty();
    }
    
    [Fact]
    public void RemoveTargetNode()
    {
        var nodes = new[]
        {
            new FlatTreeNode<string, object>("n1", "n0"),
            new FlatTreeNode<string, object>("n2", "n1"),
            new FlatTreeNode<string, object>("n3", "n2"),
            new FlatTreeNode<string, object>("n4", "n0")
        };
        
        var sut = new TreeNode<string, object>("n0");

        foreach (var node in nodes)
        {
            sut.TryAdd(node);
        }

        var nodeWillBeRemoved = sut.FindByKeyOrDefault("n2");
        var result = sut.TryRemove("n2");

        result.Should().BeTrue();

        var parentNode = sut.FindByKeyOrDefault("n1");
        parentNode!.Children.Should().BeEmpty();
    }
    
    [Fact]
    public void MoveTargetNode()
    {
        var nodes = new[]
        {
            new FlatTreeNode<string, object>("n1", "n0"),
            new FlatTreeNode<string, object>("n2", "n1"),
            new FlatTreeNode<string, object>("n3", "n2"),
            new FlatTreeNode<string, object>("n4", "n0")
        };
        
        var sut = new TreeNode<string, object>("n0");

        foreach (var node in nodes)
        {
            sut.TryAdd(node);
        }

        var nodeWillBeMoved = sut.FindByKeyOrDefault("n2");
        var result = sut.TryMove("n2", "n4");

        result.Should().BeTrue();

        var parentNode = sut.FindByKeyOrDefault("n4");
        parentNode!.Children.Should().NotBeEmpty();

        var oldParent = sut.FindByKeyOrDefault("n1");
        oldParent!.Children.Should().BeEmpty();
    }
    
    [Fact]
    public void FindDeepNodeAfterMove()
    {
        var nodes = new[]
        {
            new FlatTreeNode<string, object>("n1", "n0"),
            new FlatTreeNode<string, object>("n2", "n1"),
            new FlatTreeNode<string, object>("n3", "n2"),
            new FlatTreeNode<string, object>("n4", "n0")
        };
        
        var sut = new TreeNode<string, object>("n0");

        foreach (var node in nodes)
        {
            sut.TryAdd(node);
        }

        var result = sut.TryMove("n2", "n4");

        result.Should().BeTrue();

        var parentNode = sut.FindByKeyOrDefault("n4");
        parentNode!.Children.Should().NotBeEmpty();

        var oldParent = sut.FindByKeyOrDefault("n1");
        oldParent!.Children.Should().BeEmpty();

        var deepNode = sut.FindByKeyOrDefault("n3");

        deepNode.Should().NotBeNull();
        deepNode!.Key.Should().Be("n3");
    }
}