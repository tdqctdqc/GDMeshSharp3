
using MessagePack;

public class MoveUnitProcedure : Procedure
{
    public int UnitId { get; private set; }
    public MapPos NewPos { get; private set; }

    public static MoveUnitProcedure Construct(Unit u, MapPos newPos)
    {
        return new MoveUnitProcedure(u.Id, newPos);
    }
    [SerializationConstructor]
    private MoveUnitProcedure(int unitId, MapPos newPos)
    {
        UnitId = unitId;
        NewPos = newPos;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var unit = key.Data.Get<Unit>(UnitId);
        unit.SetPosition(NewPos, key);
    }

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(UnitId) == false)
        {
            error = "Unit not found";
            return false;
        }

        error = "";
        return true;
    }
}