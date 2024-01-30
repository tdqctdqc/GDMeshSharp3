
public class SetUnitOrderProcedure : Procedure
{
    public ERef<UnitGroup> Group { get; private set; }
    public UnitGroupOrder GroupOrder { get; private set; }

    public SetUnitOrderProcedure(ERef<UnitGroup> group, UnitGroupOrder groupOrder)
    {
        Group = group;
        GroupOrder = groupOrder;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        Group.Entity(key.Data).SetOrder(GroupOrder, key);
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}