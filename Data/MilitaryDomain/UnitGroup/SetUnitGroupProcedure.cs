
public class SetUnitGroupProcedure : Procedure
{
    public ERef<Unit> Unit { get; private set; }
    public ERef<UnitGroup> NewGroup { get; private set; }

    public SetUnitGroupProcedure(ERef<Unit> unit, ERef<UnitGroup> newGroup)
    {
        Unit = unit;
        NewGroup = newGroup;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var oldGroup = key.Data.Military.UnitAux.UnitByGroup[Unit.Entity(key.Data)];
        UnitGroup.ChangeUnitGroup(Unit.Entity(key.Data),
            oldGroup, NewGroup.Entity(key.Data), key);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}