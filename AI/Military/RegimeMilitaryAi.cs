
using System.Collections.Generic;
using System.Linq;

public class RegimeMilitaryAi
{
    private Regime _regime;
    public ForceCompositionAi ForceComposition { get; private set; }
    
    public RegimeMilitaryAi(Regime regime, Data d)
    {
        _regime = regime;
        ForceComposition = new ForceCompositionAi(regime);
        
    }
    public void CalculateMajor(LogicWriteKey key, MajorTurnOrders orders)
    {
        ForceComposition.Calculate(_regime, key);
    }

    public void CalculateMinor(LogicWriteKey key, MinorTurnOrders orders)
    {
        
    }
    
    
}