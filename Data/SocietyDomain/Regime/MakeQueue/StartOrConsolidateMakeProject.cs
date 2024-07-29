
using MessagePack;

public class StartOrConsolidateMakeProject : HostProcedure
{
    public MakeProject Project { get; private set; }

    public StartOrConsolidateMakeProject(MakeProject project)
    {
        Project = project;
    }

    public override void Enact(LogicKey key)
    {
        var regime = Project.Regime.Get(key.Data);
        var queue = regime.MakeQueue.Queue;
        Project.Start(key);
        if (queue.Count > 0
            && queue[^1].Consolidate(Project, key))
        {
            var proc = new ReplaceMakeProjectProc(queue[^1], queue[^1].Id);
            key.SendMessage(proc);
        }
        else
        {
            var proc = AddMakeProjectProc.Construct(Project.Regime,
                Project, key);
            key.SendMessage(proc);
        }
    }
    public override bool Valid(Data d, out string error)
    {
        error = "";
        return true;
    }
    public class AddMakeProjectProc : Procedure
    {
        public static AddMakeProjectProc Construct(ERef<Regime> regime, 
            MakeProject project, LogicKey key)
        {
            project.SetId(key);
            return new AddMakeProjectProc(regime, project);
        }
        [SerializationConstructor] private AddMakeProjectProc(ERef<Regime> regime, 
            MakeProject project)
        {
            Regime = regime;
            Project = project;
        }

        public ERef<Regime> Regime { get; private set; }
        public MakeProject Project { get; private set; }
        public override void Enact(ProcedureKey key)
        {
            Regime.Get(key.Data).MakeQueue.Queue.Add(Project);
        }

        public override bool Valid(Data data, out string error)
        {
            if (data.HasEntity(Regime.RefId) == false)
            {
                error = "Regime not found";
                return false;
            }
            error = "";
            return true;
        }
    }
}