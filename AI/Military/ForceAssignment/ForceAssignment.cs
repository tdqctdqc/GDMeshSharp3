
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

[MessagePack.Union(0, typeof(FrontAssignment))]
[MessagePack.Union(1, typeof(TheaterAssignment))]
[MessagePack.Union(2, typeof(FrontSegmentAssignment))]
public abstract class ForceAssignment : IPolymorph
{
    public EntityRef<Regime> Regime { get; private set; }
    public HashSet<int> GroupIds { get; private set; }
    public int Id { get; private set; }
    public IEnumerable<UnitGroup> Groups(Data d)
    {
        return GroupIds.Select(g => d.Get<UnitGroup>(g));
    }
    public abstract void CalculateOrders(MinorTurnOrders orders, LogicWriteKey key);
    [SerializationConstructor] protected 
        ForceAssignment(HashSet<int> groupIds,
            EntityRef<Regime> regime, int id)
    {
        GroupIds = groupIds;
        Regime = regime;
        Id = id;
    }

    public float GetPowerPointsAssigned(Data data)
    {
        return Groups(data).Sum(g => g.GetPowerPoints(data));
    }

    public abstract float GetPowerPointNeed(Data d);

    public float GetSatisfiedRatio(Data d)
    {
        var assigned = GetPowerPointsAssigned(d);
        var need = GetPowerPointNeed(d);
        if (need == 0f) return Mathf.Inf;
        return assigned / need;
    }

    public abstract PolyCell GetCharacteristicCell(Data d);
    public abstract UnitGroup RequestGroup(LogicWriteKey key);
    public abstract void TakeAwayGroup(UnitGroup g, LogicWriteKey key);
    public abstract void AssignGroups(LogicWriteKey key);
    public abstract void ValidateGroups(LogicWriteKey key);
}