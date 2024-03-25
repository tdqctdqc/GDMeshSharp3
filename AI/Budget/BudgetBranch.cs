
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BudgetBranch : IBudgetNode
{
    public List<IBudgetNode> Children { get; }
    public BudgetBranch Parent { get; }
    public ZeroToOne Weight { get; protected set; }
    protected BudgetBranch()
    {
        Children = new List<IBudgetNode>();
    }

    public IEnumerable<PriorityNode> GetLeaves()
    {
        var selfLeaves = Children.OfType<PriorityNode>();
        var childLeaves = Children.OfType<BudgetBranch>()
            .SelectMany(b => b.GetLeaves());
        return selfLeaves.Concat(childLeaves);
    }
    public abstract void SetWeights(float selfWeight, Regime r, Data d);
}