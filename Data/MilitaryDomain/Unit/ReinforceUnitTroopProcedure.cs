
using Godot;

public class ReinforceUnitTroopProcedure : Procedure
{
    public ERef<Unit> Unit { get; private set; }
    public ModelRef<Troop> Troop { get; private set; }

    public ReinforceUnitTroopProcedure(ERef<Unit> unit, ModelRef<Troop> troop)
    {
        Unit = unit;
        Troop = troop;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var unit = Unit.Get(key.Data);
        var template = unit.Template.Get(key.Data);
        var regime = unit.Regime.Get(key.Data);
        var reserve = regime.Stock;
        var troop = Troop.Get(key.Data);
        if (reserve.Stock.Contents.ContainsKey(troop.Id) == false)
        {
            return;
        }

        var count = unit.Troops.Get(troop);
        var need = template.Troops.Get(troop);
        if (count >= need || need == 0) return;

        var transfer = Mathf.Clamp(need - count, 
            0f, reserve.Stock.Get(troop));
        if (transfer > 0)
        {
            reserve.Stock.Remove(troop, transfer);
            unit.Troops.Add(troop, transfer);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        if (data.HasEntity(Unit.RefId) == false)
        {
            error = "unit not found";
            return false;
        }
        return true;
    }
}