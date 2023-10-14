using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAccount
{
    public IdCount<Item> Items { get; private set; }
    public IdCount<Flow> Flows { get; private set; }
    public float Labor { get; private set; }
    public HashSet<Item> UsedItem { get; private set; }
    public HashSet<Flow> UsedFlow { get; private set; }
    public bool UsedLabor { get; private set; }

    public BudgetAccount()
    {
        UsedItem = new HashSet<Item>();
        UsedFlow = new HashSet<Flow>();
        UsedLabor = false;
        Items = IdCount<Item>.Construct();
        Flows = IdCount<Flow>.Construct();
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
        foreach (var kvp in pool.AvailFlows.Contents.ToArray())
        {
            var flow = kvp.Key;
            var net = kvp.Value;
            var q = Mathf.Max(0f, share * pool.OrigFlows.Get(flow));
            q = Mathf.Min(q, pool.AvailFlows.Get(flow));
            Flows.Add(flow, q);
            pool.AvailFlows.Remove(flow, q);
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
        Flows.Remove(flow, q);
        UsedFlow.Add(flow);
    }
    public void Clear()
    {
        Labor = 0f;
        UsedItem.Clear();
        UsedFlow.Clear();
        UsedLabor = false;
    }

    public void Add(BudgetAccount toAdd)
    {
        foreach (var kvp in toAdd.Flows.Contents)
        {
            Flows.Add(kvp.Key, kvp.Value);
        }
        foreach (var kvp in toAdd.Items.Contents)
        {
            Items.Add(kvp.Key, kvp.Value);
        }

        Labor += toAdd.Labor;
    }
}
