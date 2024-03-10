
public class SetContextProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
        key.Data.Context.Calculate(key.Data);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}