
using Godot;
using MessagePack;

public class ModelMakeProject : MakeProject
{
    public static MakeProject Construct<TMakeable>(
        Regime r,
        TMakeable t,
        float amount)
        where TMakeable : class, IModel, IMakeable
    {
        return new ModelMakeProject(r.MakeRef(),
            ((IModel)t).MakeRef(),
            amount, 0f, -1);
    }
    [SerializationConstructor] protected ModelMakeProject(
        ERef<Regime> regime, 
        IdRef making, 
        float amount, float fulfilled, int id) 
        : base(regime, making, amount, fulfilled, id)
    {
    }

    public override void Start(ProcedureWriteKey key)
    {
        
    }

    public override void Increment(float amount, 
        ProductionResult result,
        LogicWriteKey key)
    {
        Fulfilled += amount;
        result.Stock.Stock.Add(Making.RefId, amount);
        result.Stock.Produced.Add(Making.RefId, amount);
    }

    public override void Finish(LogicWriteKey key)
    {
        
    }

    public override void Cancel(ProcedureWriteKey key)
    {
        
    }

    public override Control GetDisplay(Data d)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var m = (IModel)Making.Get(d);
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
        

        var costs = makeable.Makeable.BuildCosts.GetEnumModel(d);
        foreach (var (key, value) in costs)
        {
            var needed = makeable.Makeable.BuildCosts.Get(key) * Amount;
            var have = makeable.Makeable.BuildCosts.Get(key) * Fulfilled;
            vbox.CreateLabelAsChild($"{key.Name}: { have } / { needed }");
        }
        
        return vbox;
    }

    public IModel Model(Data d)
    {
        return (IModel)Making.Get(d);
    }
}