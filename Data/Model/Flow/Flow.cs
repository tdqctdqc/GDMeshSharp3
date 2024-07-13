using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Flow : Item, IIconed
{
    protected Flow()
    {
    }
    public abstract float GetNonBuildingSupply(Regime r, Data d);
}
