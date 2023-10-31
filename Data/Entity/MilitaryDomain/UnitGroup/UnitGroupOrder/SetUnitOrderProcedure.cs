
public class SetUnitOrderProcedure : Procedure
{
    public EntityRef<UnitGroup> Group { get; private set; }
    public UnitOrder Order { get; private set; }

    public SetUnitOrderProcedure(EntityRef<UnitGroup> group, UnitOrder order)
    {
        Group = group;
        Order = order;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Group.Entity(key.Data).SetOrder(Order, key);
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}