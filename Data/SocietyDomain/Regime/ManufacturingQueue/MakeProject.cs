using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
[MessagePack.Union(0, typeof(DefaultMakeProject))]
[MessagePack.Union(1, typeof(PlayerBuildingMakeProject))]

public abstract class MakeProject : IPolymorph
{
    public ERef<Regime> Regime { get; private set; }
    public ModelRef<IModel> Making { get; protected set; }
    public float Amount { get; private set; }
    public float Fulfilled { get; protected set; }

    [SerializationConstructor] protected MakeProject(
        ERef<Regime> regime, 
        ModelRef<IModel> making,
        float amount, float fulfilled)
    {
        Regime = regime;
        Amount = amount;
        Making = making;
        Fulfilled = fulfilled;
    }

    public abstract void Increment(float amount, ProcedureWriteKey key);

    public abstract void Finish(ProcedureWriteKey key);
    public Control GetDisplay(Data d)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var m = Making.Get(d);
        var makeable = (IMakeable)m;
        var vbox = new VBoxContainer();
        if (m is IIconed i)
        {
            var icon = i.Icon.GetLabeledIcon<HBoxContainer>(
                $"{m.Name}: {Fulfilled} / {Amount} ",
                size);
            vbox.AddChild(icon);
        }
        else
        {
            vbox.CreateLabelAsChild(m.Name);
        }
        

        var costs = makeable.Makeable.BuildCosts.GetEnumerableModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = makeable.Makeable.BuildCosts.Get(key) * Amount;
            var have = makeable.Makeable.BuildCosts.Get(key) * Fulfilled;
            vbox.CreateLabelAsChild($"{key.Name}: { have } / { needed }");
        }
        
        return vbox;
    }
}