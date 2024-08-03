
using System.Collections.Generic;

public class SetFrontlineAdvanceIntoProcedure : Procedure
{
    public ERef<Frontline> Frontline { get; private set; }
    public HashSet<CellRef> AdvanceInto { get; private set; }

    public SetFrontlineAdvanceIntoProcedure(ERef<Frontline> frontline, HashSet<CellRef> advanceInto)
    {
        Frontline = frontline;
        AdvanceInto = advanceInto;
    }

    public override void Enact(ProcedureKey key)
    {
        var fl = Frontline.Get(key.GetData());
        fl.AdvanceInto.Clear();
        fl.AdvanceInto.UnionWith(AdvanceInto);
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}