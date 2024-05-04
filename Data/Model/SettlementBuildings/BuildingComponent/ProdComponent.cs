
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ProdComponent : BuildingModelComponent
{
    public IdCount<IModel> Inputs { get; private set; }
    public IdCount<IModel> Outputs { get; private set; }
    public IdCount<PeepJob> Jobs { get; private set; }

    public ProdComponent(IdCount<IModel> inputs,
        IdCount<IModel> outputs,
        IdCount<PeepJob> jobs)
    {
        Inputs = inputs;
        Outputs = outputs;
        Jobs = jobs;
    }

    public override void Work(Cell cell, float staffingRatio, 
        ProcedureWriteKey key)
    {
        
    }

}