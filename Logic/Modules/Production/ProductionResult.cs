
using System.Collections.Generic;

public class ProductionResult
{
    public ERef<Regime> Regime { get; private set; }
    public RegimeStock Stock { get; private set; }
    public Dictionary<int, int> PeepGrowths { get; private set; }
    public Dictionary<ModelRef<IModel>, float> Made { get; private set; }
    public ProductionResult(ERef<Regime> regime, 
        RegimeStock stock, 
        Dictionary<int, int> peepGrowths,
        Dictionary<ModelRef<IModel>, float> made)
    {
        Regime = regime;
        Stock = stock;
        PeepGrowths = peepGrowths;
        Made = made;
    }
}