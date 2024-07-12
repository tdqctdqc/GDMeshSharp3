using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Flow : Item, IIconed
{
    protected Flow(string name)
        : base(name)
    {
    }
    public abstract float GetNonBuildingSupply(Regime r, Data d);
}
