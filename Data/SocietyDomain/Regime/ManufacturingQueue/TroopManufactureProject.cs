
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TroopManufactureProject : ManufactureProject
{
    public ModelRef<Troop> Troop { get; private set; }
    public TroopManufactureProject(int id, float ipProgress, float amount, 
        ModelRef<Troop> troop) : base(id, ipProgress, amount)
    {
        Troop = troop;
    }
    public override float IndustrialCost(Data d)
    {
        return Troop.Model(d).Makeable.IndustrialCost * Amount;
    }
    protected override Icon GetIcon(Data d)
    {
        return Troop.Model(d).Icon;
    }
    public override IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d)
    {
        return Troop.Model(d).Makeable.ItemCosts.GetEnumerableModel(d)
            .Select(kvp => new KeyValuePair<Item, int>(kvp.Key, 
                Mathf.FloorToInt(kvp.Value * Amount)));
    }
    public override void Work(Regime r, ProcedureWriteKey key, float ip)
    {
        if (ip < 0) throw new Exception();
        IpProgress += ip;
        var item = Troop.Model(key.Data);
        var itemCost = item.Makeable.IndustrialCost;
        var amountProd = ip / itemCost;
        r.Military.TroopReserve.Add(item, amountProd);
    }
}