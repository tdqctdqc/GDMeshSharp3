
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeMilitaryAi
{
    private ERef<Regime> _regime;
    public ForceCompositionAi ForceComposition { get; private set; }
    public UnitTemplatesAi Templates { get; private set; }


    public RegimeMilitaryAi(ERef<Regime> regime, ForceCompositionAi forceComposition, UnitTemplatesAi templates)
    {
        _regime = regime;
        ForceComposition = forceComposition;
        Templates = templates;
    }

    public static RegimeMilitaryAi Construct(Regime regime, Data d)
    {
        return new RegimeMilitaryAi(regime.MakeRef(),
            new ForceCompositionAi(new Dictionary<UnitTemplatesAi.UnitTypeTag, int>()),
        UnitTemplatesAi.Construct(regime, d));
    }
    public void CalculateMajor(LogicKey key, MajorTurnOrders orders)
    {
        ForceComposition.Calculate(_regime.Get(key.Data), key);
        Templates.Calculate(key);
    }

    public void CalculateMinor(LogicKey key, MinorTurnOrders orders)
    {
        
    }
    
    
}