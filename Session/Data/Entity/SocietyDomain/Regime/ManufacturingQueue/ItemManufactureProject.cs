
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemManufactureProject : ManufactureProject
{
    public float Amount { get; private set; }
    public ModelRef<Item> Item { get; private set; }

    public ItemManufactureProject(int id, float progress, float amount, ModelRef<Item> item) : base(id, progress)
    {
        if (Amount < 0) throw new Exception();
        Amount = amount;
        Item = item;
    }

    public override float IndustrialCost(Data d)
    {
        return Item.Model(d).Attributes.Get<MakeableAttribute>().IndustrialCost * Amount;
    }

    public override IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d)
    {
        return Item.Model(d).Attributes.Get<MakeableAttribute>().ItemCosts
            .Select(kvp => new KeyValuePair<Item, int>(kvp.Key, Mathf.FloorToInt(kvp.Value * Amount)));
    }

    public override Control GetDisplay(Data d)
    {
        var hbox = new HBoxContainer();
        var icon = Item.Model(d).Icon;
        var texture = icon.GetTextureRect(Vector2.One * 50f);
        hbox.AddChild(texture);
        var completedRatio = Progress / IndustrialCost(d);
        
        hbox.CreateLabelAsChild($"{(completedRatio * Amount).RoundTo2Digits()}/{Amount.RoundTo2Digits()}");
        
        
        return hbox;
    }
    public override void Work(Regime r, ProcedureWriteKey key, float ip)
    {
        if (ip < 0) throw new Exception();
        Progress += ip;
        var item = Item.Model(key.Data);
        var itemCost = item.Attributes.Get<MakeableAttribute>().IndustrialCost;
        var amountProd = ip / itemCost;
        r.Items.Add(item, amountProd);
    }
}