using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MakeProject
{
    public ERef<Regime> Regime { get; private set; }
    public ModelRef<IModel> Making { get; protected set; }
    public int Amount { get; private set; }
    public IdCount<IModel> Fulfilled { get; private set; }

    public static MakeProject Construct<TMakeable>(
        Regime r,
        TMakeable t,
        int amount)
        where TMakeable : class, IModel, IMakeable
    {
        return new MakeProject(r.MakeRef(),
            ((IModel)t).MakeRef(),
            amount, IdCount<IModel>.Construct());
    }
    [SerializationConstructor] private MakeProject(
        ERef<Regime> regime, 
        ModelRef<IModel> making,
        int amount, IdCount<IModel> fulfilled)
    {
        Regime = regime;
        Amount = amount;
        Making = making;
        Fulfilled = fulfilled;
    }
    public virtual void Finish(ProcedureWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        regime.Stock.Stock.Add(Making.Get(key.Data), Amount);
    }
    public Control GetDisplay(Data d)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var m = Making.Get(d);
        var makeable = (IMakeable)m;
        var vbox = new VBoxContainer();
        if (m is IIconed i)
        {
            var icon = i.Icon.GetLabeledIcon<HBoxContainer>(
                $"{m.Name}",
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
            vbox.CreateLabelAsChild($"{key.Name}: {Fulfilled.Get(key)} / {makeable.Makeable.BuildCosts.Get(key)}");
        }
        
        return vbox;
    }
}