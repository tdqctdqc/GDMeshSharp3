using System;
using System.Collections.Generic;
using System.Linq;

public class Tree<T> : ITree
{
    public Dictionary<Type, TreeNode<T>> Nodes { get; private set; }

    public Tree()
    {
        Nodes = new Dictionary<Type, TreeNode<T>>();
    }
}
