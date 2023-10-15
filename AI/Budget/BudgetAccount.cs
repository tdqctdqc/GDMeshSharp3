using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAccount
{
    public IdCount<Item> Items { get; private set; }
    public IdCount<IModel> Models { get; private set; }
    public float Labor { get; private set; }
    public HashSet<Item> UsedItem { get; private set; }
    public HashSet<IModel> UsedModel { get; private set; }
    public bool UsedLabor { get; private set; }

    public BudgetAccount()
    {
        UsedItem = new HashSet<Item>();
        UsedModel = new HashSet<IModel>();
        UsedLabor = false;
        Items = IdCount<Item>.Construct();
        Models = IdCount<IModel>.Construct();
        Labor = 0f;
    }
    public void TakeShare(float share, BudgetPool pool, Data data)
    {
        foreach (var kvp in pool.AvailItems.Contents.ToArray())
        {
            var item = kvp.Key;
            var q = Mathf.Min(pool.AvailItems.Get(item), share * pool.OrigItems.Get(item));
            Items.Add(item, q);
            pool.AvailItems.Remove(item, q);
        }
        foreach (var kvp in pool.AvailModels.Contents.ToArray())
        {
            var flow = kvp.Key;
            var net = kvp.Value;
            var q = Mathf.Max(0f, share * pool.OrigModels.Get(flow));
            q = Mathf.Min(q, pool.AvailModels.Get(flow));
            Models.Add(flow, q);
            pool.AvailModels.Remove(flow, q);
        }

        var labor = pool.OrigLabor * share;
        Labor += labor;
        pool.AvailLabor -= labor;
    }

    public void UseLabor(float labor)
    {
        if (labor == 0f) return;
        Labor -= labor;
        UsedLabor = true;
    }
    
    public void UseItem(Item item, float q)
    {
        Items.Remove(item, q);
        UsedItem.Add(item);
    }
    public void UseFlow(Flow flow, float q)
    {
        Models.Remove(flow, q);
        UsedModel.Add(flow);
    }
    public void Clear()
    {
        Labor = 0f;
        UsedItem.Clear();
        UsedModel.Clear();
        UsedLabor = false;
    }

    public void Add(BudgetAccount toAdd)
    {
        foreach (var kvp in toAdd.Models.Contents)
        {
            Models.Add(kvp.Key, kvp.Value);
        }
        foreach (var kvp in toAdd.Items.Contents)
        {
            Items.Add(kvp.Key, kvp.Value);
        }

        Labor += toAdd.Labor;
    }
}
