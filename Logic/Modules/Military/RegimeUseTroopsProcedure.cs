using System.Collections.Generic;
using Godot;
using MessagePack;

public class RegimeUseTroopsProcedure : Procedure
{
    public EntityRef<Regime> Regime { get; private set; }
    public Dictionary<int, float> UsageByTroopId { get; private set; }

    public static RegimeUseTroopsProcedure Construct(Regime r)
    {
        return new RegimeUseTroopsProcedure(r.MakeRef(), 
            new Dictionary<int, float>());
    }
    [SerializationConstructor] private RegimeUseTroopsProcedure
        (EntityRef<Regime> regime, Dictionary<int, float> usageByTroopId)
    {
        Regime = regime;
        UsageByTroopId = usageByTroopId;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var regimeTroops = Regime.Entity(key.Data).Military.TroopReserve;
        foreach (var kvp in UsageByTroopId)
        {
            regimeTroops.Remove(kvp.Key, kvp.Value);
        }
    }

    public void AddTroopCosts(UnitTemplate template, int num, Data d)
    {
        foreach (var (troop, numTroop) in template.TroopCounts
                                                        .GetEnumerableModel(d))
        {
            UsageByTroopId.AddOrSum(troop.Id, numTroop * num);
        }
    }
    public override bool Valid(Data data)
    {
        return true;
    }
}