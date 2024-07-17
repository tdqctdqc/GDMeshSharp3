
public class ConstructBuildingsBudgetBranch
    : BudgetBranch
{
    private PriorityNode _industrial, _income, _research;
    public ConstructBuildingsBudgetBranch(Regime regime,
        BudgetBranch parent, 
        Data d)
        : base("Construct Buildings")
    {
        Parent = parent;
        var industrial =
            new MakeProductionBuildingsPriority(
                d.Models.Items.IndustrialPower,
                regime,
                "Make Industrial");
        _industrial = new PriorityNode(industrial, this,
            (d, r) => 1f);
        Children.Add(_industrial);
        
        var income = new MakeProductionBuildingsPriority(
            d.Models.Items.Income,
            regime,
            "Make Income");
        _income = new PriorityNode(income, this,
            (d, r) => 0f);
        
        var research = new MakeProductionBuildingsPriority(
            d.Models.Items.Research,
            regime,
            "Make Research");
        _research = new PriorityNode(research, this,
            (d, r) => .5f);
        
        Children.Add(_income);
    }

    protected override float GetWeight(Regime r, Data d)
    {
        return 1f;
    }
}