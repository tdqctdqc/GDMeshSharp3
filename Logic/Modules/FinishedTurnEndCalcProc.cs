
public class FinishedTurnEndCalcProc : Procedure
{
    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.Notices.FinishedTurnEndCalc.Invoke();
    }
}