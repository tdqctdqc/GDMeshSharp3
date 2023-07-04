using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemHistory : MultiCountHistory<int>
{
    public static ItemHistory Construct()
    {
        return new ItemHistory(new Dictionary<int, CountHistory>());
    }
    [SerializationConstructor] private ItemHistory(Dictionary<int, CountHistory> dic) 
        : base(dic)
    {
        
    }
    public void TakeSnapshot(int tick, ItemWallet wallet)
    {
        foreach (var kvp in wallet.Contents)
        {
            var item = kvp.Key;
            var amt = kvp.Value;
            if(Counts.ContainsKey(item) == false) Counts.Add(item, CountHistory.Construct());
            Counts[item].Add(amt, tick);
        }
    }

    public void Add(Item item, int value, int tick)
    {
        if(Counts.ContainsKey(item.Id) == false) Counts.Add(item.Id, CountHistory.Construct());
        Counts[item.Id].Add(value, tick);
    }
}
