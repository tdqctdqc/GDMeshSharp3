using System;
using System.Collections.Generic;
using System.Linq;

public class Items : ModelManager<Item>
{
    public Food Food { get; private set; } = new ();
    public Recruits Recruits { get; private set; } = new ();
    public Iron Iron { get; private set; } = new ();
    public Oil Oil { get; private set; } = new ();
    public FinancialPower FinancialPower { get; private set; } = new ();
    public Coal Coal { get; private set; } = new ();
    public HeavyMetal HeavyMetal { get; private set; } = new ();
    public Income Income { get; private set; } 
        = new ();
    public IndustrialPower IndustrialPower { get; private set; } 
        = new ();
    public ConstructionCap ConstructionCap { get; private set; } 
        = new ();
    public MilitaryCap MilitaryCap { get; private set; }
        = new ();
    public Labor Labor { get; private set; }
        = new();

    public Research Research { get; private set; }
        = new ();
}
