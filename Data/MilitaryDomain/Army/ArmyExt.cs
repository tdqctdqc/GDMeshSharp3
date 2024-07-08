
using System;
using Godot;

public static class ArmyExt
{
    public static ArmyTree GetTree(this Army a, int startColumn,
        Client c)
    {
        return new ArmyTree(a, 0, c);
    }
}