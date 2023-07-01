using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class Market : Entity
{
    public Dictionary<int, float> ItemPricesById { get; private set; }
    
    public static Market Create(CreateWriteKey key)
    {
        var prices = key.Data.Models.Items.Models.Values
            .SelectWhereOfType<Item, TradeableItem>()
            .ToDictionary(item => item.Id, item => item.DefaultPrice);
        var m = new Market(key.IdDispenser.GetID(), prices);
        key.Create(m);
        return m;
    }
    [SerializationConstructor] private Market(int id, Dictionary<int, float> itemPricesById) : base(id)
    {
        ItemPricesById = itemPricesById;
    }

    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(SocietyDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
}
