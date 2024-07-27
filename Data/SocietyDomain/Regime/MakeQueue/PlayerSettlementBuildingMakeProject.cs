
using Godot;
using MessagePack;

public class PlayerSettlementBuildingMakeProject : MakeProject
{
    public ERef<Settlement> Settlement { get; private set; }
    public ModelRef<SettlementBuilding> Building { get; private set; }
    public static PlayerSettlementBuildingMakeProject Construct(
        int amount,
        Settlement settlement,
        Regime regime,
        SettlementBuilding making)
    {
        return new PlayerSettlementBuildingMakeProject(
            settlement.MakeRef(), regime.MakeRef(),
            making.MakeRef(),
            amount, 0f, -1);
    }
    [SerializationConstructor] protected PlayerSettlementBuildingMakeProject(
        ERef<Settlement> settlement,
        ERef<Regime> regime, 
        ModelRef<SettlementBuilding> building,
        float amount,
        float fulfilled, int id) 
            : base(regime, amount, fulfilled, id)
    {
        Building = building;
        Settlement = settlement;
    }
    
    public override void Start(LogicWriteKey key)
    {
        var regime = Regime.Get(key.Data);
        Increment(regime.Stock, key);
        var setStock = new SetStockProcedure(regime.MakeRef(),
            regime.Stock);
        key.SendMessage(setStock);
    }

    public override void Increment(
        RegimeStock stock,
        LogicWriteKey key)
    {
        var before = Mathf.FloorToInt(Fulfilled);
        Fulfilled += BuildTree.Increment(GetMakeable(key.Data),
            stock,
            Amount - Fulfilled,
            key);;
        var after = Mathf.FloorToInt(Fulfilled);
        var diff = after - before;
        var building = Building.Get(key.Data);
        var settlement = Settlement.Get(key.Data);
        for (var i = 0; i < diff; i++)
        {
            var regime = Regime.Get(key.Data);
            var proc = new AddSettlementBuildingProcedure(Settlement, building.MakeRef());
            key.SendMessage(proc);
        }
    }

    public override void Finish(LogicWriteKey key)
    {
        
    }

    public override void Cancel(ProcedureWriteKey key)
    {
        var making = Building.Get(key.Data);
        var stock = Regime.Get(key.Data).Stock;
        foreach (var (model, amt) in making.Makeable.BuildCosts.GetEnumModel(key.Data))
        {
            var spent = Fulfilled * amt;
            stock.Stock.Add(model, spent);
        }
    }


    public override Control GetDisplay(Data d)
    {
        var size = Game.I.Client.Settings.MedIconSize.Value;
        var m = Building.Get(d);
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
        if (next is not PlayerSettlementBuildingMakeProject p
            || p.Settlement.Equals(Settlement) == false
            || p.Building.Equals(Building) == false)
        {
            return false;
        }

        Amount += p.Amount;
        var before1 = Mathf.FloorToInt(Fulfilled);
        var before2 = Mathf.FloorToInt(next.Fulfilled);
        var before = before1 + before2;
        var after = Mathf.FloorToInt(Fulfilled + next.Fulfilled);
        var diff = after - before;
        var building = Building.Get(key.Data);
        var settlement = Settlement.Get(key.Data);
        for (var i = 0; i < diff; i++)
        {
            var regime = Regime.Get(key.Data);
            var proc = new AddSettlementBuildingProcedure(Settlement, building.MakeRef());
            key.SendMessage(proc);
        }

        Fulfilled += next.Fulfilled;
        return true;
    }

    public override MakeableAttribute GetMakeable(Data d)
    {
        return Building.Get(d).Makeable;
    }

    public override Icon GetIcon(Data d)
    {
        return Building.Get(d).Icon;
    }

    public override string Description(Data d)
    {
        return Building.Get(d).Name;
    }
}