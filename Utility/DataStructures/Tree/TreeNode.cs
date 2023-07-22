using System;
using System.Collections.Generic;
using System.Linq;

public class TreeNode<T>
{
    public TreeNode<T> Parent { get; private set; }
    public HashSet<TreeNode<T>> Children { get; private set; }
    public T Value { get; private set; }

    public TreeNode(T value)
    {
        Value = value;
        Children = new HashSet<TreeNode<T>>();
        Parent = null;
    }

    public void SetParent(TreeNode<T> parent)
    {
        if (Parent != null) Parent.Children.Remove(this);
        Parent = parent;
        Parent.Children.Add(this);
    }
}
