
public class AddCombatHistoryProc : Procedure
{
    public CombatHistory History { get; private set; }

    public AddCombatHistoryProc(CombatHistory history)
    {
        History = history;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.Military.CombatHistories.Value.Histories
            .Add(History.Tick, History);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}