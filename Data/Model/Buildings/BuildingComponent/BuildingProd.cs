
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BuildingProd : BuildingModelComponent
{
    public IdCount<IModel> Inputs { get; private set; }
    public IdCount<IModel> Outputs { get; private set; }
    public IdCount<PeepJob> Jobs { get; private set; }

    public BuildingProd(IdCount<IModel> inputs,
        IdCount<IModel> outputs,
        IdCount<PeepJob> jobs,
        FlowList flows)
    {
        Inputs = inputs;
        Outputs = outputs;
        Jobs = jobs;

        var jobSum = Jobs.Contents.Sum(kvp => kvp.Value);
        var laborSum = inputs.Get(flows.Labor);
        if (jobSum != laborSum) throw new Exception();
    }

    public override void Work(Cell cell, float staffingRatio, 
        ProcedureWriteKey key)
    {
        
    }

}