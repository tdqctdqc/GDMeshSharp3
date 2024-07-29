
using Godot;
using MessagePack;

public class ModelMakeProject : MakeProject
{
    public ModelRef<IModel> Model { get; private set; }
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
        ModelRef<IModel> model, 
        float amount, float fulfilled, int id) 
        : base(regime, amount, fulfilled, id)
    {
        Model = model;
    }


    public override void Start(LogicKey key)
    {
        
    }

    public override void Increment(
        RegimeStock stock,
        LogicKey key)
    {
        var amount = BuildTree.Increment(GetMakeable(key.Data),
            stock,
            Amount - Fulfilled,
            key);
        Fulfilled += amount;
        stock.Stock.Add(Model.RefId, amount);
        stock.Produced.Add(Model.RefId, amount);
    }

    public override void Finish(LogicKey key)
    {
    }

    public override void Cancel(ProcedureKey key)
    {
        
    }

    public override Control GetDisplay(Data d)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var m = Model.Get(d);
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

    public override bool Consolidate(MakeProject next, LogicKey key)
    {
        if (next is ModelMakeProject p == false
            || p.Model.Equals(Model) == false)
        {
            return false;
        }

        Fulfilled += p.Fulfilled;
        Amount += p.Amount;
        return true;
    }

    public override MakeableAttribute GetMakeable(Data d)
    {
        return ((IMakeable)Model.Get(d)).Makeable;
    }

    public override Icon GetIcon(Data d)
    {
        var model = Model.Get(d);
        if (model is IIconed i) return i.Icon;
        return Icon.Blank;
    }

    public override string Description(Data d)
    {
        return Model.Get(d).Name;
    }
}