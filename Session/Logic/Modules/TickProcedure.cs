
using Godot;

public class TickProcedure : Procedure
{
    public override bool Valid(Data data)
    {
        return true;
    }
    public override void Enact(ProcedureWriteKey key)
    {
        var gc = key.Data.BaseDomain.GameClock;
        var tick = gc.Tick + 1;
        gc.Set<int>(nameof(GameClock.Tick), tick, key);
        key.Data.Notices.Ticked.Invoke(gc.Tick);
    }
}
