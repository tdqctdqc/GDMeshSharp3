
public class SetUnitGroupProcedure : Procedure
{
    public EntityRef<Unit> Unit { get; private set; }
    public EntityRef<UnitGroup> NewGroup { get; private set; }

    public SetUnitGroupProcedure(EntityRef<Unit> unit, EntityRef<UnitGroup> newGroup)
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

    public override bool Valid(Data data)
    {
        return true;
    }
}