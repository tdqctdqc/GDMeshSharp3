
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ItemManufactureProject : ManufactureProject
{
    public float Amount { get; private set; }
    public ModelRef<Item> Item { get; private set; }

    public ItemManufactureProject(int id, float progress, float amount) : base(id, progress)
    {
        Amount = amount;
    }

    public override float IndustrialCost(Data d)
    {
        return Item.Model(d).Attributes.Get<ProduceableAttribute>().IndustrialCost * Amount;
    }

    public override IEnumerable<KeyValuePair<Item, int>> ItemCosts(Data d)
    {
        return Item.Model(d).Attributes.Get<ProduceableAttribute>().ItemCosts
            .Select(kvp => new KeyValuePair<Item, int>(kvp.Key, Mathf.FloorToInt(kvp.Value * Amount)));
    }
    protected override void Complete(Regime r, ProcedureWriteKey key)
    {
        r.Items.Add(Item.Model(key.Data), Amount);
    }
}