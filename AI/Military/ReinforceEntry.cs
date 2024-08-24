
public struct ReinforceEntry
{
    public Unit Unit { get; private set; }
    public Troop Troop { get; private set; }
    public float Amount { get; private set; }

    public ReinforceEntry(Unit unit, Troop troop, float amount)
    {
        Unit = unit;
        Troop = troop;
        Amount = amount;
    }
}