using System.Collections.Generic;


public class SetTheaterCellsProcedure : Procedure
{
    public ERef<Theater> Theater { get; private set; }
    public HashSet<CellRef> Cells { get; private set; }

    public SetTheaterCellsProcedure(ERef<Theater> theater, HashSet<CellRef> cells)
    {
        Theater = theater;
        Cells = cells;
    }

    public override void Enact(ProcedureKey key)
    {
        var t = Theater.Get(key.Data);
        t.Cells.Clear();
        t.Cells.UnionWith(Cells);
    }

    public override bool Valid(Data data, out string error)
    {
        if (data.HasEntity(Theater.RefId) == false)
        {
            error = "Theater not found";
            return false;
        }

        error = "";
        return true;
    }
}