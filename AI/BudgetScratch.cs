using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetScratch
{
    public ItemCount Items { get; private set; }
    public FlowCount Flows { get; private set; }
    public float Unemployed { get; private set; }

    public BudgetScratch()
    {
        Items = new ItemCount(new Dictionary<int, float>());
        Flows = new FlowCount(new Dictionary<int, float>());
        Unemployed = 0f;
    }

    public void TakeShare(float share, ItemCount totalItems, FlowCount totalFlows,
        int totalUnemployed)
    {
        foreach (var kvp in totalItems.Contents)
        {
            Items.Add(kvp.Key, share * kvp.Value);
        }
        foreach (var kvp in totalFlows.Contents)
        {
            var flowShare = share * kvp.Value;
            Flows.Add(kvp.Key, Mathf.Max(0f, flowShare));
        }

        Unemployed += totalUnemployed * share;
    }
}
