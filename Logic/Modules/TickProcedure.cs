
using Godot;

public class TickProcedure : Procedure
{
    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
    public override void Enact(ProcedureKey key)
    {
        key.Data.Context.Calculate(key.Data);
        key.Data.BaseDomain.GameClock.DoTick(key);
    }
}
