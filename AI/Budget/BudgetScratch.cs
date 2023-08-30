using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class BudgetScratch
{
    public ItemCount Items { get; private set; }
    public FlowCount Flows { get; private set; }
    public float Unemployed { get; private set; }
    public float Credit { get; private set; }

    public BudgetScratch()
    {
        Items = new ItemCount(new Dictionary<int, float>());
        Flows = new FlowCount(new Dictionary<int, float>());
        Unemployed = 0f;
        Credit = 0f;
    }

    public void TakeShare(float share, ItemCount totalItems, RegimeFlows totalFlows,
        int totalUnemployed, int totalCredit)
    {
        foreach (var kvp in totalItems.Contents)
        {
            Items.Add(kvp.Key, share * kvp.Value);
        }
        foreach (var kvp in totalFlows.Flows)
        {
            var net = kvp.Value.Net();
            var flowShare = share * net;
            Flows.Add(kvp.Key, Mathf.Max(0f, flowShare));
        }

        Unemployed += totalUnemployed * share;
        Credit += totalCredit * share;
    }

    public void SubtractCredit(float credit)
    {
        Credit -= credit;
    }

    public void MaxCredit()
    {
        Credit = float.MaxValue;
    }
}
