using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ManufactureProject
{
    public int Id { get; private set; }
    public float Progress { get; private set; }
    public abstract float IndustrialCost(Data d);
    public abstract IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d);

    protected ManufactureProject(int id, float progress)
    {
        Id = id;
        Progress = progress;
    }

    public void Work(Regime r, ProcedureWriteKey key, float ip)
    {
        Progress += ip;
        if (Progress >= IndustrialCost(key.Data))
        {
            Complete(r, key);
        }
    }
    protected abstract void Complete(Regime r, ProcedureWriteKey key);

    public bool IsComplete(Data d)
    {
        return Progress >= IndustrialCost(d);
    }
    public float Remaining(Data d)
    {
        return Mathf.Min(0f, Progress - IndustrialCost(d));
    }
}