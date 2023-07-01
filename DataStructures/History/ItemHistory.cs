using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class ItemHistory : MultiCountHistory<int>
{
    public static ItemHistory Construct(Data data)
    {
        var dic = new Dictionary<int, CountHistory>();
        foreach (var v in data.Models.Items.Models.Values)
        {
            dic.Add(v.Id, CountHistory.Construct());
        }
        return new ItemHistory(dic);
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
            Counts[item].Add(amt, tick);
        }
    }
}
