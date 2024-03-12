
using System;
using Godot;

public class StartMakeProjectProc : Procedure
{
    public StartMakeProjectProc(ERef<Regime> regime, MakeProject project)
    {
        Regime = regime;
        Project = project;
    }

    public ERef<Regime> Regime { get; private set; }
    public MakeProject Project { get; private set; }
    public override void Enact(ProcedureWriteKey key)
    {
        Regime.Get(key.Data).MakeQueue.Queue.Enqueue(Project);
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