
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class LaborComponent : BuildingModelComponent
{
    public IdCount<Item> Inputs { get; private set; }
    public IdCount<Item> Outputs { get; private set; }
    public IdCount<PeepJob> Jobs { get; private set; }

    public LaborComponent(IdCount<Item> inputs,
        IdCount<Item> outputs,
        IdCount<PeepJob> jobs)
    {
        Inputs = inputs;
        Outputs = outputs;
        Jobs = jobs;
    }

    public float TotalLabor() => Jobs.Contents.Values.Sum();

}