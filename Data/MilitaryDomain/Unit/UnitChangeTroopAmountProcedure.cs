
public class UnitChangeTroopAmountProcedure : Procedure
{
    public float Amount { get; private set; }
    public ModelRef<Troop> Troop { get; private set; }
    public ERef<Unit> Unit { get; private set; }

    public UnitChangeTroopAmountProcedure(float amount, ModelRef<Troop> troop, ERef<Unit> unit)
    {
        Amount = amount;
        Troop = troop;
        Unit = unit;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var u = Unit.Get(key.Data);
        var t = Troop.Get(key.Data);
        if (Amount > 0f)
        {
            u.Troops.Add(t, Amount);
        }
        else
        {
            u.Troops.Remove(t, -Amount);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(Unit.RefId) == false)
        {
            error = "Unit not found";
            return false;
        }
        error = "";
        return true;
    }
}