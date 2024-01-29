
using Godot;
using MessagePack;

public class DestroyUnitProcedure : Procedure
{
    public int UnitId { get; private set; }

    public static DestroyUnitProcedure Construct(Unit u)
    {
        return new DestroyUnitProcedure(u.Id);
    }
    [SerializationConstructor]
    private DestroyUnitProcedure(int unitId)
    {
        UnitId = unitId;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var unit = key.Data.Get<Unit>(UnitId);
        var group = unit.GetGroup(key.Data);
        var cell = unit.Position.GetCell(key.Data);
        if (group != null)
        {
            group.Units.Remove(unit, key);
        }
        key.Data.RemoveEntity(unit.Id, key);
        GD.Print("destroyed unit at " + cell.Id);
        if (group.Units.Count() == 0)
        {
            GD.Print("removing empty group at " + cell.Id);
            key.Data.RemoveEntity(group.Id, key);
        }
    }

    public override bool Valid(Data data)
    {
        return data.Get<Unit>(UnitId) != null;
    }
}