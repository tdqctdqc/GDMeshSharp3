
using System.Collections.Generic;

public abstract class ForceAssignment : IPolymorph
{
    public HashSet<UnitGroup> Groups { get; private set; }
    public abstract void CalculateOrders(MinorTurnOrders orders, Data data);
}