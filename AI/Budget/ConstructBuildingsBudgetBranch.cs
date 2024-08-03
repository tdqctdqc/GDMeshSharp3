
using System.Collections.Generic;

public class ConstructBuildingsBudgetBranch
    : BudgetBranch
{

    public static ConstructBuildingsBudgetBranch Construct(Regime regime,
        BudgetRoot root, Data d)
    {
        var b = new ConstructBuildingsBudgetBranch(new List<IBudgetNode>(),
            0f, "Construct Buildings");
        var industrialPriority =
            MakeIndustrialBuildingsPriority.Construct(d);
        var industrialNode = new PriorityNode(industrialPriority);
        root.SetParent(industrialNode, b);
        
        var researchPriority = MakeResearchBuildingsPriority.Construct(d);
        var researchNode = new PriorityNode(researchPriority);
        root.SetParent(researchNode, b);

        return b;
    }
    
    public ConstructBuildingsBudgetBranch(List<IBudgetNode> children, float weight, string name) : base(children, weight, name)
    {
    }

    protected override float GetWeight(Regime r, BudgetRoot root, Data d)
    {
        return 1f;
    }
}