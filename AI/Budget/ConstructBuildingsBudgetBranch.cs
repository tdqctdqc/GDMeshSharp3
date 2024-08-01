
using System.Collections.Generic;

public class ConstructBuildingsBudgetBranch
    : BudgetBranch
{

    public static ConstructBuildingsBudgetBranch Construct(Regime regime, Data d)
    {
        var b = new ConstructBuildingsBudgetBranch(new List<IBudgetNode>(),
            0f, "Construct Buildings");
        var industrialPriority =
            new MakeProductionBuildingsPriority(
                d.Models.Items.IndustrialPower.MakeRef<IModel>(),
                regime,
                "Make Industrial");
        var industrialNode = new PriorityNode(industrialPriority,
            (d, r) => 1f);
        b.Children.Add(industrialNode);
        
        var incomePriority = new MakeProductionBuildingsPriority(
            d.Models.Items.Income.MakeRef<IModel>(),
            regime,
            "Make Income");
        var incomeNode = new PriorityNode(incomePriority,
            (d, r) => 0f);
        b.Children.Add(incomeNode);

        var researchPriority = new MakeProductionBuildingsPriority(
            d.Models.Items.Research.MakeRef<IModel>(),
            regime,
            "Make Research");
        var researchNode = new PriorityNode(researchPriority,
            (d, r) => .5f);
        b.Children.Add(researchNode);


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