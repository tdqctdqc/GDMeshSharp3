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
    public void Remove(Item item, float amount)
    {
        if (amount == 0) return;
        Remove(item.Id, amount);
    }

    public static ItemCount Union(params ItemCount[] counts)
    {
        var res = ItemCount.Construct();
        foreach (var count in counts)
        {
            foreach (var kvp in count.Contents)
            {
                res.Add(kvp.Key, kvp.Value);
            }
        }

        return res;
    }

    public void Subtract(ItemCount take)
    {
        foreach (var kvp in take.Contents)
        {
            Remove(kvp.Key, kvp.Value);
        }
    }
}
