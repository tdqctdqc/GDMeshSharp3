using System;
using System.Collections.Generic;
using System.Linq;

public class Items : ModelList<Item>
{
    public Food Food { get; private set; } = new ();
    public Recruits Recruits { get; private set; } = new ();
    public Iron Iron { get; private set; } = new ();
    public Oil Oil { get; private set; } = new ();
    public FinancialPower FinancialPower { get; private set; } = new ();
    public Coal Coal { get; private set; } = new ();
    public HeavyMetal HeavyMetal { get; private set; } = new ();
}
