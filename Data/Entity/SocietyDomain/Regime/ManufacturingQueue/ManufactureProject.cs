using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

[MessagePack.Union(0, typeof(ItemManufactureProject))]
[MessagePack.Union(1, typeof(TroopManufactureProject))]
public abstract class ManufactureProject : IPolymorph
{
    public int Id { get; protected set; }
    public float Amount { get; private set; }
    public float IpProgress { get; protected set; }
    public abstract float IndustrialCost(Data d);
    public abstract IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d);
    

    protected ManufactureProject(int id, float ipProgress, float amount)
    {
        Amount = amount;
        if (Amount < 0) throw new Exception();
        Id = id;
        IpProgress = ipProgress;
    }
    protected abstract Icon GetIcon(Data d);
    public Control GetDisplay(Data d)
    {
        var icon = GetIcon(d);
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var completedRatio = IpProgress / IndustrialCost(d);
        var box = icon.GetLabeledIcon<HBoxContainer>(
            $"{(completedRatio * Amount).RoundTo2Digits()}/{Amount.RoundTo2Digits()}",
            size);
        return box;
    }
    public abstract void Work(Regime r, ProcedureWriteKey key, float ip);
    public bool IsComplete(Data d)
    {
        return IpProgress >= IndustrialCost(d);
    }
    public float Remaining(Data d)
    {
        return Mathf.Max(0f, IndustrialCost(d) - IpProgress);
    }
}