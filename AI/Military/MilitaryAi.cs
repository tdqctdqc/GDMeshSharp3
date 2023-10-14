
public class MilitaryAi
{
    private Regime _regime;
    public ForceCompositionAi ForceComposition { get; private set; }
    

    public MilitaryAi(Regime regime)
    {
        _regime = regime;
        ForceComposition = new ForceCompositionAi();
    }

    public void Calculate(Data data, MajorTurnOrders orders)
    {
        var reserve = IdCount<Troop>.Construct(_regime.TroopReserve);
        ForceComposition.Calculate(_regime, data, orders, reserve);
    }
}