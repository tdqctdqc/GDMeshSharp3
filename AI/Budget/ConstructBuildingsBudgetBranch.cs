
public class ConstructBuildingsBudgetBranch
    : BudgetBranch
{
    private PriorityNode _industrial, _income;
    public ConstructBuildingsBudgetBranch(Data d)
        : base("Construct Buildings")
    {
        var industrial =
            new MakeProductionBuildingsPriority(
                d.Models.Flows.IndustrialPower,
                "Make Industrial");
        _industrial = new PriorityNode(industrial, this,
            (d, r) => 1f);
        Children.Add(_industrial);
        
        var income = new MakeProductionBuildingsPriority(
            d.Models.Flows.Income,
            "Make Income");
        _income = new PriorityNode(income, this,
            (d, r) => 0f);
        Children.Add(_income);
    }

    protected override float GetWeight(Regime r, Data d)
    {
        return 1f;
    }
}