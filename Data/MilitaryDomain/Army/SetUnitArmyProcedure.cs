
public class SetUnitArmyProcedure : Procedure
{
    public ERef<Unit> Unit { get; private set; }
    public ERef<Army> NewGroup { get; private set; }

    public SetUnitArmyProcedure(ERef<Unit> unit, ERef<Army> newGroup)
    {
        Unit = unit;
        NewGroup = newGroup;
    }

    public override void Enact(ProcedureKey key)
    {
        var oldGroup = key.Data.Military.UnitAux.UnitByGroup[Unit.Get(key.Data)];
        Army.ChangeUnitGroup(Unit.Get(key.Data),
            oldGroup, NewGroup.Get(key.Data), key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}