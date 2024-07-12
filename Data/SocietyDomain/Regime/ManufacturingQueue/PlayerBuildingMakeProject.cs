
using Godot;
using MessagePack;

public class PlayerBuildingMakeProject : MakeProject
{
    public ERef<Settlement> Settlement { get; private set; }
    
    public static PlayerBuildingMakeProject Construct(
        int amount,
        Settlement settlement,
        Regime regime,
        SettlementBuilding making)
    {
        return new PlayerBuildingMakeProject(
            settlement.MakeRef(), regime.MakeRef(),
            making.MakeRef<IModel>(),
            amount, 0f, -1);
    }
    [SerializationConstructor] protected PlayerBuildingMakeProject(
        ERef<Settlement> settlement,
        ERef<Regime> regime, 
        IdRef making,
        float amount,
        float fulfilled, int id) 
            : base(regime, making, amount, fulfilled, id)
    {
        Settlement = settlement;
    }
    
    public override void Start(LogicWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        var before = Mathf.FloorToInt(Fulfilled);
        var increment = BuildTree.Increment(this, regime.Stock, key);
        if(increment == 0f) return;
        Fulfilled += increment;
        var after = Mathf.FloorToInt(Fulfilled);
        
        var diff = after - before;
        for (var i = 0; i < diff; i++)
        {
            var building = (SettlementBuilding)Making.Get(key.Data);
            var settlement = Settlement.Get(key.Data);
            var proc = new AddBuildingProcedure(Settlement, building.MakeRef());
            key.SendMessage(proc);
        }
        var setStock = new SetStockProcedure(regime.MakeRef(),
            regime.Stock);
        key.SendMessage(setStock);
    }

    public override void Increment(float amount, 
        RegimeStock stock,
        LogicWriteKey key)
    {
        var before = Mathf.FloorToInt(Fulfilled);
        Fulfilled += amount;
        var after = Mathf.FloorToInt(Fulfilled);
        var diff = after - before;
        for (var i = 0; i < diff; i++)
        {
            var building = (SettlementBuilding)Making.Get(key.Data);
            var settlement = Settlement.Get(key.Data);
            var regime = Regime.Get(key.Data);
            var proc = new AddBuildingProcedure(Settlement, building.MakeRef());
            key.SendMessage(proc);
        }
    }

    public override void Finish(LogicWriteKey key)
    {
        
    }

    public override void Cancel(ProcedureWriteKey key)
    {
        var making = MakingBuilding(key.Data);
        var stock = Regime.Get(key.Data).Stock;
        foreach (var (model, amt) in making.Makeable.BuildCosts.GetEnumModel(key.Data))
        {
            var spent = Fulfilled * amt;
            stock.Stock.Add(model, spent);
        }
    }

    public SettlementBuilding MakingBuilding(Data d)
    {
        return (SettlementBuilding)Making.Get(d);
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

    public override bool Consolidate(MakeProject next, 
        LogicWriteKey key)
    {
        if (next is not PlayerBuildingMakeProject p
            || p.Settlement.Equals(Settlement) == false
            || p.Making.RefId != Making.RefId)
        {
            return false;
        }

        Amount += p.Amount;
        var before1 = Mathf.FloorToInt(Fulfilled);
        var before2 = Mathf.FloorToInt(next.Fulfilled);
        var before = before1 + before2;
        var after = Mathf.FloorToInt(Fulfilled + next.Fulfilled);
        var diff = after - before;
        for (var i = 0; i < diff; i++)
        {
            var building = (SettlementBuilding)Making.Get(key.Data);
            var settlement = Settlement.Get(key.Data);
            var regime = Regime.Get(key.Data);
            var proc = new AddBuildingProcedure(Settlement, building.MakeRef());
            key.SendMessage(proc);
        }

        Fulfilled += next.Fulfilled;
        return true;
    }
}