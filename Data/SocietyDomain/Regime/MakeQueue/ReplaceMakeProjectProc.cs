
using System.Linq;

public class ReplaceMakeProjectProc : Procedure
{
    public MakeProject Project { get; private set; }
    public int IdToReplace { get; private set; }

    public ReplaceMakeProjectProc(MakeProject project, int idToReplace)
    {
        Project = project;
        IdToReplace = idToReplace;
    }

    public override void Enact(ProcedureWriteKey key)
    {
        var queue = Project.Regime.Get(key.Data)
            .MakeQueue.Queue;
        var index = queue.FindIndex(p => p.Id == IdToReplace);
        queue[index] = Project;
    }

    public override bool Valid(Data data, out string error)
    {
        var regime = Project.Regime.Get(data);
        if (regime.MakeQueue.Queue.Any(p => p.Id == IdToReplace))
        {
            error = "";
            return true;
        }

        error = "could not find project to replace";
        return false;
    }
}