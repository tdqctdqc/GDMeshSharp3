
using System.Linq;

public class ChangeMakeProjectPriorityProcedure 
    : Procedure
{
    public ERef<Regime> Regime { get; private set; }
    public int ProjectId { get; private set; }
    public int NewIndex { get; private set; }

    public ChangeMakeProjectPriorityProcedure(ERef<Regime> regime, int projectId, int newIndex)
    {
        Regime = regime;
        ProjectId = projectId;
        NewIndex = newIndex;
    }

    public override void Enact(ProcedureKey key)
    {
        var regime = Regime.Get(key.Data);
        var makeQueue = regime.MakeQueue.Queue;
        var proj = makeQueue.First(p => p.Id == ProjectId);
        makeQueue.Remove(proj);
        makeQueue.Insert(NewIndex, proj);
    }

    public override bool Valid(Data data, out string error)
    {
        var regime = Regime.Get(data);
        var makeQueue = regime.MakeQueue.Queue;
        if (makeQueue.Any(p => p.Id == ProjectId))
        {
            error = "";
            return true;
        }

        error = "Project not found";
        return false;
    }
}