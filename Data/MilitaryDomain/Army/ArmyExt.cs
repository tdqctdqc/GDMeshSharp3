
using System;
using Godot;

public static class ArmyExt
{
    public static ArmyTree GetTree(this Army a, int startColumn,
        Data d)
    {
        return new ArmyTree(a, 0, d);
    }
}