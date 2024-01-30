
using System;
using Godot;

public class StartManufacturingProjectProc : Procedure
{
    public StartManufacturingProjectProc(ERef<Regime> regime, ManufactureProject project)
    {
        Regime = regime;
        Project = project;
    }

    public ERef<Regime> Regime { get; private set; }
    public ManufactureProject Project { get; private set; }
    public override void Enact(ProcedureWriteKey key)
    {
        try
        {
            foreach (var kvp2 in Project.ItemCosts(key.Data))
            {
                Regime.Entity(key.Data).Items.Remove(kvp2.Key, kvp2.Value);
            }
            Regime.Entity(key.Data).ManufacturingQueue.Queue.Enqueue(Project);
        }
        catch (Exception e)
        {
            key.Data.Logger.Log("problem enacting manuf project " + Project.GetType().Name, LogType.Logic);
        }
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}