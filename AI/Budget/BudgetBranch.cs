
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BudgetBranch : IBudgetNode
{
    public List<IBudgetNode> Children { get; }
    public BudgetBranch Parent { get; }
    public float Weight { get; protected set; }
    public string Name { get; private set; }
    protected BudgetBranch(string name)
    {
        Name = name;
        Children = new List<IBudgetNode>();
    }

    public IEnumerable<PriorityNode> GetLeaves()
    {
        var selfLeaves = Children.OfType<PriorityNode>();
        var childLeaves = Children.OfType<BudgetBranch>()
            .SelectMany(b => b.GetLeaves());
        return selfLeaves.Concat(childLeaves);
    }

    public void SetWeights(Regime r, Data d)
    {
        Weight = GetWeight(r, d);
        foreach (var child in Children)
        {
            child.SetWeights(r, d);
        }
    }

    protected abstract float GetWeight(Regime r, Data d);
}