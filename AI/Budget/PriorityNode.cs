
using System;
using System.Collections.Generic;
using System.Linq;

public class PriorityNode : IBudgetNode
{
    public string Name => Priority.Name;
    public CreditBuffer Credit { get; set; }
    public IBudgetPriority Priority { get; private set; }
    public BudgetBranch Parent { get; }
    public Dictionary<int, IdCount<IModel>> MadeByTick { get; private set; }
    public float Weight { get; private set; }
    private Func<Regime, Data, float> _getWeight;
    

    public PriorityNode(IBudgetPriority priority, 
        BudgetBranch parent,
        Func<Regime, Data, float> getWeight)
    {
        Priority = priority;
        Parent = parent;
        Credit = new CreditBuffer(20);
        _getWeight = getWeight;
        MadeByTick = new Dictionary<int, IdCount<IModel>>();
        
    }
    public void SetWeights(Regime r, Data d)
    {
        Weight = _getWeight(r, d);
    }
}