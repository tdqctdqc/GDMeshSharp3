using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Flow : IModel
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public static Income Income { get; private set; } = new ();

    protected Flow(string name)
    {
        Name = name;
    }

    public abstract float GetNonBuildingFlow(Regime r, Data d);
}
