
using Godot;

public class TickProcedure : Procedure
{
    public override bool Valid(Data data)
    {
        return true;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.BaseDomain.GameClock.DoTick(key);
    }
}
