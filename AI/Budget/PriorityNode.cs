
using System;
using System.Collections.Generic;
using System.Linq;

public class PriorityNode : IBudgetNode
{
    public string Name => Priority.Name;
    public CreditBuffer Credit { get; set; }
    public IBudgetPriority Priority { get; private set; }
    public Dictionary<int, Dictionary<string, float>> MadeByTick { get; private set; }
    public float Weight { get; private set; }
    private Func<Regime, Data, float> _getWeight;

    public PriorityNode(Func<Regime, Data, float> getWeight, 
        CreditBuffer credit, IBudgetPriority priority, 
        Dictionary<int, Dictionary<string, float>> madeByTick, float weight)
    {
        _getWeight = getWeight;
        Credit = credit;
        Priority = priority;
        MadeByTick = madeByTick;
        Weight = weight;
    }


    public PriorityNode(IBudgetPriority priority, 
        Func<Regime, Data, float> getWeight)
    {
        Priority = priority;
        Credit = new CreditBuffer(20);
        _getWeight = getWeight;
        MadeByTick = new Dictionary<int, Dictionary<string, float>>();
    }
    public void SetWeights(Regime r, BudgetRoot root, Data d)
    {
        Weight = _getWeight(r, d);
    }
}