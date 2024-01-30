
using System.Collections.Generic;
using System.Linq;

public class RegimeMilitaryAi
{
    private Regime _regime;
    public ForceCompositionAi ForceComposition { get; private set; }
    public DeploymentAi Deployment { get; private set; }

    public RegimeMilitaryAi(Regime regime, Data d)
    {
        _regime = regime;
        ForceComposition = new ForceCompositionAi(regime);
        Deployment = DeploymentAi.Construct(regime, d);
    }

    
    public void CalculateMajor(LogicWriteKey key, MajorTurnOrders orders)
    {
        var reserve = IdCount<Troop>.Construct(_regime.Military.TroopReserve);
        ForceComposition.Calculate(_regime, key, orders, reserve);
    }

    public void CalculateMinor(LogicWriteKey key, MinorTurnOrders orders)
    {
        Deployment.Calculate(_regime, key, orders);
    }
    
    
}