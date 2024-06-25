
using Godot;

public class AddCombatHistoryProc : Procedure
{
    public int Tick { get; private set; }
    public CombatGraph History { get; private set; }
    
    public AddCombatHistoryProc(int tick, CombatGraph history)
    {
        Tick = tick;
        History = history;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.Military.CombatHistories.Value.Histories
            .Add(Tick, History);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}