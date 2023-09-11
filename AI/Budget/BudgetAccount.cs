using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetAccount
{
    public ItemCount Items { get; private set; }
    public FlowCount Flows { get; private set; }
    public float Labor { get; private set; }

    public BudgetAccount()
    {
        Items = new ItemCount(new Dictionary<int, float>());
        Flows = new FlowCount(new Dictionary<int, float>());
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
            Flows.Add(flow, q);
            pool.AvailFlows.Remove(flow, q);
        }

        var labor = pool.OrigLabor * share;
        Labor += labor;
        pool.AvailLabor -= labor;
    }

    public void UseLabor(float labor)
    {
        Labor -= labor;
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
