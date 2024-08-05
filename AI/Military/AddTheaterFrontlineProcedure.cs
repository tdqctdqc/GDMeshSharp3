
public class AddTheaterFrontlineProcedure : Procedure
{
    public ERef<Theater> Theater { get; private set; }
    public ERef<Frontline> Frontline { get; private set; }

    public AddTheaterFrontlineProcedure(ERef<Theater> theater, ERef<Frontline> frontline)
    {
        Theater = theater;
        Frontline = frontline;
    }

    public override void Enact(ProcedureKey key)
    {
        Theater.Get(key.Data).Frontlines.Add(Frontline, key);
    }

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(Theater.RefId) == false)
        {
            error = "couldnt find theater";
            return false;
        }

        if (data.HasEntity(Frontline.RefId) == false)
        {
            error = "couldnt find frontline";
            return false;
        }

        error = "";
        return true;
    }
}