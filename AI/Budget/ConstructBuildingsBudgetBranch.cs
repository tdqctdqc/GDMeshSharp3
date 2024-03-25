
public class ConstructBuildingsBudgetBranch
    : BudgetBranch
{
    private PriorityNode _industrial, _income;
    public ConstructBuildingsBudgetBranch(Data d)
    {
        var industrial =
            new MakeProductionBuildingsPriority(
                d.Models.Flows.IndustrialPower,
                "Make Industrial",
                (d, r) => 1f);
        _industrial = new PriorityNode(industrial, this);
        Children.Add(_industrial);
        
        var income = new MakeProductionBuildingsPriority(
            d.Models.Flows.Income,
            "Make Income",
            (d, r) => 1f);
        _income = new PriorityNode(income, this);
        Children.Add(_income);
    }

    public override void SetWeights(float selfWeight, Regime r, Data d)
    {
        Weight = new ZeroToOne(selfWeight);
        _industrial.SetWeight(.9f);
        _income.SetWeight(.1f);
    }
}