
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemManufactureProject : ManufactureProject
{
    public ModelRef<Item> Item { get; private set; }

    public ItemManufactureProject(int id, float ipProgress, float amount, 
        ModelRef<Item> item) : base(id, ipProgress, amount)
    {
        Item = item;
    }

    public override float IndustrialCost(Data d)
    {
        var m = ((IMakeable)Item.Model(d)).Makeable;
        return m.IndustrialCost * Amount;
    }

    public override IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d)
    {
        var m = ((IMakeable)Item.Model(d)).Makeable;
        return m.ItemCosts.GetEnumerableModel(d)
            .Select(kvp => new KeyValuePair<Item, int>(kvp.Key, Mathf.FloorToInt(kvp.Value * Amount)));
    }

    protected override Icon GetIcon(Data d)
    {
        return Item.Model(d).Icon;
    }

    public override void Work(Regime r, ProcedureWriteKey key, float ip)
    {
        if (ip < 0) throw new Exception();
        IpProgress += ip;
        var item = Item.Model(key.Data);
        var m = ((IMakeable)Item.Model(key.Data)).Makeable;
        var itemCost = m.IndustrialCost;
        var amountProd = ip / itemCost;
        r.Items.Add(item, amountProd);
    }
}