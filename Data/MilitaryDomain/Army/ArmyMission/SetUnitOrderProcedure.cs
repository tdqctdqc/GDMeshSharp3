
public class SetUnitOrderProcedure : Procedure
{
    public ERef<Army> Group { get; private set; }
    public ArmyMission GroupMission { get; private set; }

    public SetUnitOrderProcedure(ERef<Army> group, ArmyMission groupMission)
    {
        Group = group;
        GroupMission = groupMission;
    }

    public override void Enact(ProcedureKey key)
    {
        if (GroupMission is LineMission l)
        {
            Group.Get(key.Data).SetLineOrder(l, key);
        }
        else
        {
            Group.Get(key.Data).AddOrder(GroupMission, key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}