
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BudgetBranch : IBudgetNode
{
    public List<IBudgetNode> Children { get; }
    public float Weight { get; protected set; }
    public string Name { get; private set; }

    protected BudgetBranch(List<IBudgetNode> children, float weight, string name)
    {
        Children = children;
        Weight = weight;
        Name = name;
    }

    public IEnumerable<PriorityNode> GetLeaves()
    {
        var selfLeaves = Children.OfType<PriorityNode>();
        var childLeaves = Children.OfType<BudgetBranch>()
            .SelectMany(b => b.GetLeaves());
        return selfLeaves.Concat(childLeaves);
    }

    public void SetWeights(Regime r, BudgetRoot root, Data d)
    {
        Weight = GetWeight(r, root, d);
        foreach (var child in Children)
        {
            child.SetWeights(r, root, d);
        }
    }

    protected abstract float GetWeight(Regime r, BudgetRoot root, Data d);
}