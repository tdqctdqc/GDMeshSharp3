
public class AggregateProcedure : Procedure
{
    public Procedure[] Procedures { get; private set; }

    public AggregateProcedure(Procedure[] procedures)
    {
        Procedures = procedures;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        for (var i = 0; i < Procedures.Length; i++)
        {
            Procedures[i].Enact(key);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        for (var i = 0; i < Procedures.Length; i++)
        {
            if (Procedures[i].Valid(data, out var e) == false)
            {
                error = e;
                return false;
            }
        }

        error = "";
        return true;
    }
}