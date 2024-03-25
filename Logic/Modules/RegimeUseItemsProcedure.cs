
using System.Collections.Generic;
using Godot;
using MessagePack;

public class RegimeUseItemsProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public Dictionary<int, int> UsageByItemId { get; private set; }

    public static RegimeUseItemsProcedure Construct(Regime r)
    {
        return new RegimeUseItemsProcedure(r.MakeRef(), new Dictionary<int, int>());
    }
    [SerializationConstructor] private RegimeUseItemsProcedure(ERef<Regime> regime, Dictionary<int, int> usageByItemId)
    {
        Regime = regime;
        UsageByItemId = usageByItemId;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var regimeItems = Regime.Get(key.Data).Stock;
        foreach (var kvp in UsageByItemId)
        {
            regimeItems.Stock.Remove(kvp.Key, kvp.Value);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}