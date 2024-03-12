using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class IndustrialPower : Flow
{
    public IndustrialPower() 
        : base(nameof(IndustrialPower))
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        return 0f;
    }
}
