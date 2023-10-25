
public class RegimeMilitaryAi
{
    private Regime _regime;
    public ForceCompositionAi ForceComposition { get; private set; }
    public DeploymentAi Deployment { get; private set; }

    public RegimeMilitaryAi(Regime regime)
    {
        _regime = regime;
        ForceComposition = new ForceCompositionAi(regime);
        Deployment = new DeploymentAi();
    }
    
    public void CalculateMajor(Data data, MajorTurnOrders orders)
    {
        var reserve = IdCount<Troop>.Construct(_regime.Military.TroopReserve);
        ForceComposition.Calculate(_regime, data, orders, reserve);
        Deployment.CalculateMajor(_regime, data, orders);
    }

    public void CalculateMinor(Data data, MinorTurnOrders orders)
    {
        Deployment.CalculateMinor(_regime, data, orders);
    }
}