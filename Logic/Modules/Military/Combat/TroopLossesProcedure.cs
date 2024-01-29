
using System.Collections.Generic;
using Godot;
using MessagePack;

public class TroopLossesProcedure : Procedure
{
    public int UnitId { get; private set; }
    public List<(int troopId, float losses)> Losses { get; private set; }

    public static TroopLossesProcedure Construct(Unit u)
    {
        return new TroopLossesProcedure(u.Id, new List<(int troopId, float losses)>());
    }
    [SerializationConstructor] private TroopLossesProcedure(int unitId, List<(int troopId, float losses)> losses)
    {
        UnitId = unitId;
        Losses = losses;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var unit = key.Data.Get<Unit>(UnitId);
        if (unit == null) return;
        foreach (var (troopId, losses) in Losses)
        {
            var actualLoss = Mathf.Min(unit.Troops.Get(troopId), losses);
            unit.Troops.Remove(troopId, actualLoss);
        }
    }

    public override bool Valid(Data data)
    {
        return data.Get<Unit>(UnitId) is Unit u;
    }
}