using System;
using System.Collections.Generic;
using System.Linq;

public class FlowList : ModelList<Flow>
{
    public Income Income { get; private set; } = new ();
    public IndustrialPower IndustrialPower { get; private set; } 
        = new ();
    public ConstructionCap ConstructionCap { get; private set; } 
        = new ();
    public MilitaryCap MilitaryCap { get; private set; }
        = new ();

    public Labor Labor { get; private set; }
        = new();
}
