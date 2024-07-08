
using System;
using System.Linq;

public class PriorityNode : IBudgetNode
{
    public string Name => Priority.Name;
    public CreditBuffer Credit { get; set; }
    public IBudgetPriority Priority { get; private set; }
    public BudgetBranch Parent { get; }
    public float Weight { get; private set; }
    private Func<Regime, Data, float> _getWeight;
    public void SetWeights(Regime r, Data d)
    {
        Weight = _getWeight(r, d);
    }

    public PriorityNode(IBudgetPriority priority, 
        BudgetBranch parent,
        Func<Regime, Data, float> getWeight)
    {
        Priority = priority;
        Parent = parent;
        Credit = new CreditBuffer(20);
        _getWeight = getWeight;
    }
}