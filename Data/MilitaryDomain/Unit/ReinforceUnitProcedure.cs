
using System.Collections.Generic;
using Godot;
using MessagePack;

public class ReinforceUnitProcedure : Procedure
{
    public static ReinforceUnitProcedure Construct(Regime regime)
    {
        return new ReinforceUnitProcedure(regime.MakeRef(), new List<(int unitId, int troopId, float count)>());
    }
    [SerializationConstructor] private ReinforceUnitProcedure(ERef<Regime> regime, List<(int unitId, int troopId, float count)> reinforceCounts)
    {
        Regime = regime;
        ReinforceCounts = reinforceCounts;
    }

    public ERef<Regime> Regime { get; private set; }
    public List<(int unitId, int troopId, float count)> 
        ReinforceCounts { get; private set; }
    public override void Enact(ProcedureWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        var reserve = regime.Stock;
        foreach (var (unitId, troopId, count) in ReinforceCounts)
        {
            if (key.Data.HasEntity(unitId) == false) continue;
            var unit = key.Data.Get<Unit>(unitId);
            var troop = key.Data.Models.GetModel<Troop>(troopId);
            if (reserve.Stock.Contents.ContainsKey(troop.Id) == false) continue;
            var transfer = Mathf.Clamp(count, 0f, reserve.Stock.Get(troop));
            if (transfer > 0)
            {
                reserve.Stock.Remove(troop, transfer);
                unit.Troops.Add(troop, transfer);
            }
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}