
using System.Collections.Generic;
using System.Linq;

[MessagePack.Union(0, typeof(FrontAssignment))]
public abstract class ForceAssignment : IPolymorph
{
    public HashSet<UnitGroup> Groups { get; private set; }
    public abstract void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key);

    public ForceAssignment()
    {
        Groups = new HashSet<UnitGroup>();
    }

    public float GetPowerPointsAssigned(Data data)
    {
        return Groups.Sum(g => g.GetPowerPoints(data));
    }
}