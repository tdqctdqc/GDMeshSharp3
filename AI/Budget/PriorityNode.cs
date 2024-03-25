
using System;

public class PriorityNode : IBudgetNode
{
    public CreditBuffer Credit { get; set; }
    public IBudgetPriority Priority { get; private set; }
    public BudgetBranch Parent { get; }
    public ZeroToOne Weight { get; private set; }

    public PriorityNode(IBudgetPriority priority, 
        BudgetBranch parent)
    {
        Priority = priority;
        Parent = parent;
        Credit = new CreditBuffer(20);
    }

    public void SetWeight(float weight)
    {
        Weight = new ZeroToOne(weight);
    }
    public float GetTreeWeight(Data d)
    {
        var mult = 1f;
        var parent = Parent;
        while (parent != null)
        {
            mult *= parent.Weight.Value;
            parent = parent.Parent;
        }
        
        return Weight.Value * mult;
    }
}