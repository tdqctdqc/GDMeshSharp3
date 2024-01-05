using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SettlementTier : IModel
{
    public string Name { get; }
    public int Id { get; private set; }
    public int MinSize { get; private set; }
    public int NumTris { get; private set; }
    public Icon Icon { get; private set; }

    public SettlementTier(string name, int minSize, int numTris)
    {
        NumTris = numTris;
        Name = name;
        MinSize = minSize;
        Icon = Icon.Create(Name, Vector2I.One);
    }
}
