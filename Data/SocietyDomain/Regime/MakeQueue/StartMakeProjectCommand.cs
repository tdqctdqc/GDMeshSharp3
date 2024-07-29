
using System;
using Godot;

public class StartMakeProjectCommand : Command
{
    public MakeProject Project { get; private set; }
    public StartMakeProjectCommand(
        MakeProject project,
        Guid commandingPlayerGuid) 
        : base(commandingPlayerGuid)
    {
        Project = project;
    }

    public override bool Valid(Data data, out string error)
    {
        error = "";
        return true;
    }

    public override void Enact(LogicKey key)
    {
        var regime = key.Data.BaseDomain
            .PlayerAux.ByGuid[CommandingPlayerGuid].Regime;
        var proc = new StartOrConsolidateMakeProject(Project);
        key.SendMessage(proc);
    }
}