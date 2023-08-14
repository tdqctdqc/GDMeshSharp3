using System;
using System.Collections.Generic;
using System.Linq;

public class Items : ModelList<Item>
{
    public Food Food { get; private set; } = new Food();
    public Recruits Recruits { get; private set; } = new Recruits();
    public Iron Iron { get; private set; } = new Iron();
    public Oil Oil { get; private set; } = new Oil();
    public FinancialPower FinancialPower { get; private set; } = new FinancialPower();
    public Coal Coal { get; private set; } = new Coal();
    public HeavyMetal HeavyMetal { get; private set; } = new HeavyMetal();
}
