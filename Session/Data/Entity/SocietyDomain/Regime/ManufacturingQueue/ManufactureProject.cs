using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[MessagePack.Union(0, typeof(ItemManufactureProject))]
public abstract class ManufactureProject : IPolymorph
{
    public int Id { get; protected set; }
    public float Progress { get; protected set; }
    public abstract float IndustrialCost(Data d);
    public abstract IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d);
    public abstract Control GetDisplay(Data d);

    protected ManufactureProject(int id, float progress)
    {
        Id = id;
        Progress = progress;
    }
    public abstract void Work(Regime r, ProcedureWriteKey key, float ip);
    public bool IsComplete(Data d)
    {
        return Progress >= IndustrialCost(d);
    }
    public float Remaining(Data d)
    {
        return Mathf.Max(0f, IndustrialCost(d) - Progress);
    }
}