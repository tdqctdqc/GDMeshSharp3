
using System.Collections.Generic;

public class ProductionResult
{
    public ERef<Regime> Regime { get; private set; }
    public RegimeStock Stock { get; private set; }
    public Dictionary<int, int> PeepGrowths { get; private set; }
    public List<MakeProject> MakeQueue { get; private set; }
    public ProductionResult(ERef<Regime> regime, 
        RegimeStock stock, 
        Dictionary<int, int> peepGrowths,
        List<MakeProject> makeQueue)
    {
        Regime = regime;
        Stock = stock;
        PeepGrowths = peepGrowths;
        MakeQueue = makeQueue;
    }
}