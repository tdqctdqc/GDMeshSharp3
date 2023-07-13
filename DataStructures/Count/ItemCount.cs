using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemCount : Count<int>
{
    public float this[Item item] => this[item.Id];
    public static ItemCount Construct()
    {
        return new ItemCount(new Dictionary<int, float>());
    }
    public static ItemCount Construct(ItemCount toCopy)
    {
        return new ItemCount(new Dictionary<int, float>(toCopy.Contents));
    }
    [SerializationConstructor] public ItemCount(Dictionary<int, float> contents) : base(contents, false)
    {
    }

    public void Add(Item item, float amount)
    {
        if (amount == 0) return;
        Add(item.Id, amount);
    }
    public void Remove(Item item, int amount)
    {
        if (amount == 0) return;
        Remove(item.Id, amount);
    }
}
