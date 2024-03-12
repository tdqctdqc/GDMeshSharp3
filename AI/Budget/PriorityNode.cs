
using System;

public class PriorityNode : IBudgetNode
{
    public CreditBuffer Credit { get; set; }
    public IBudgetPriority Priority { get; private set; }
    public BudgetBranch Parent { get; }
    private Func<Data, float> _calcWeight;

    public PriorityNode(IBudgetPriority priority, BudgetBranch parent,
        Func<Data, float> calcWeight)
    {
        _calcWeight = calcWeight;
        Priority = priority;
        Parent = parent;
        Credit = new CreditBuffer(20);
    }

    public float GetWeight(Data d)
    {
        return _calcWeight(d);
    }

    public float GetTreeWeight(Data d)
    {
        var mult = 1f;
        var parent = Parent;
        while (parent != null)
        {
            mult *= parent.GetWeight();
            parent = parent.Parent;
        }

        return GetWeight(d) * mult;
    }
}