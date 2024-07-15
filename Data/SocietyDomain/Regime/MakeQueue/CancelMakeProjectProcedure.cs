
using System.Linq;

public class CancelMakeProjectProcedure : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public int MakeProjectId { get; private set; }

    public CancelMakeProjectProcedure(ERef<Regime> regime, int makeProjectId)
    {
        Regime = regime;
        MakeProjectId = makeProjectId;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var queue = Regime.Get(key.Data).MakeQueue;
        var proj = queue.Queue
            .FirstOrDefault(p => p.Id == MakeProjectId);
        if (proj is not null)
        {
            proj.Cancel(key);
            queue.Queue.Remove(proj);
        }
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }
}