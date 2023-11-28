
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

[MessagePack.Union(0, typeof(FrontAssignment))]
[MessagePack.Union(1, typeof(TheaterAssignment))]
public abstract class ForceAssignment : IPolymorph
{
    public EntityRef<Regime> Regime { get; private set; }
    public HashSet<int> GroupIds { get; private set; }

    public IEnumerable<UnitGroup> Groups(Data d)
    {
        return GroupIds.Select(g => d.Get<UnitGroup>(g));
    }
    public abstract void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key);
    [SerializationConstructor] protected 
        ForceAssignment(HashSet<int> groupIds,
            EntityRef<Regime> regime)
    {
        GroupIds = groupIds;
        Regime = regime;
    }

    public float GetPowerPointsAssigned(Data data)
    {
        return Groups(data).Sum(g => g.GetPowerPoints(data));
    }
}