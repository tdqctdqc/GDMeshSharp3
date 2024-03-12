
using System;
using System.Collections.Generic;
using System.Linq;

public abstract class BudgetBranch : IBudgetNode
{
    IEnumerable<IBudgetNode> Children { get; }
    public BudgetBranch Parent { get; }

    public IEnumerable<PriorityNode> GetLeaves()
    {
        var selfLeaves = Children.OfType<PriorityNode>();
        var childLeaves = Children.OfType<BudgetBranch>()
            .SelectMany(b => b.GetLeaves());
        return selfLeaves.Concat(childLeaves);
    }

    public abstract float GetWeight();
}