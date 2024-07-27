
using System;
using Godot;

public class UpgradeTroopProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public ModelRef<Troop> From { get; private set; }
    public ModelRef<Troop> To { get; private set; }
    public float Amount { get; private set; }
    public ERef<Unit> Target { get; private set; }

    public UpgradeTroopProcedure(ERef<Regime> regime, ModelRef<Troop> from, ModelRef<Troop> to, float amount, ERef<Unit> target)
    {
        Regime = regime;
        From = from;
        To = to;
        Amount = amount;
        Target = target;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        var from = From.Get(key.Data);
        var to = To.Get(key.Data);
        if(Target.IsEmpty())
        {
            var amt = Mathf.Min(Amount, regime.Stock.Stock.Get(from));
            regime.Stock.Stock.Add(to, amt);
            regime.Stock.Stock.Remove(from, amt);
        }
        else
        {
            var unit = Target.Get(key.Data);
            var amt = Mathf.Min(Amount, unit.Troops.Get(from));
            unit.Troops.Remove(from, amt);
            unit.Troops.Add(to, amt);
        }
    }

    public override bool Valid(Data d, out string error)
    {
        error = "";
        return true;
    }
}