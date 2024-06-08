
using System;
using Godot;

public static class ArmyExt
{
    public static ArmyTree GetTree(this Army a, Data d)
    {
        var tree = new ArmyTree();
        tree.Setup(a, d);
        return tree;
    }
}