
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

    public static void Enact(Regime regime,
        List<ReinforceEntry> reinforceEntries,
        IWriteKey key)
    {
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

        if (key is LogicKey l && l.HasRemotes())
        {
            var proc = new ReinforceProcedure(
                regime.MakeRef(),
                reinforceEntries.Select(
                    v => (v.Unit.Id, v.Troop.Id, v.Amount)).ToList()
            );
            l.SendMessage(proc);
        }
    }

    public override void Enact(ProcedureKey key)
    {
        var entries = ReinforceCounts.Select(
            v => new ReinforceEntry(key.Data.Get<Unit>(v.unitId),
                key.Data.Models.GetModel<Troop>(v.troopId),
                v.count
            )).ToList();
        Enact(Regime.Get(key.Data), entries, key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}