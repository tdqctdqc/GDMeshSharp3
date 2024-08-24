
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class ReinforceProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public List<(int unitId, int troopId, float count)> 
        ReinforceCounts { get; private set; }
    [SerializationConstructor] public ReinforceProcedure(ERef<Regime> regime, List<(int unitId, int troopId, float count)> reinforceCounts)
    {
        Regime = regime;
        ReinforceCounts = reinforceCounts;
    }

    public override void Enact(ProcedureKey key)
    {
        var reinforceEntries = ReinforceCounts.Select(
            v => new ReinforceEntry(key.Data.Get<Unit>(v.unitId),
                key.Data.Models.GetModel<Troop>(v.troopId),
                v.count
            )).ToList();
        var regime = Regime.Get(key.Data);
        var reserve = regime.Stock;
        var data = key.GetData();
        foreach (var entry in reinforceEntries)
        {
            if (data.HasEntity(entry.Unit.Id) == false) continue;
            var unit = entry.Unit;
            var troop = entry.Troop;
            if (reserve.Stock.Contents.ContainsKey(troop.Id) == false) continue;
            var transfer = Mathf.Clamp(entry.Amount, 0f, reserve.Stock.Get(troop));
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