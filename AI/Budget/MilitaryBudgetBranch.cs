
public class MilitaryBudgetBranch
    : BudgetBranch
{
    private PriorityNode _recruits, _reinforcements;
    public MilitaryBudgetBranch(Data d)
    {
        var recruits = new MakeProductionBuildingsPriority(
            d.Models.Items.Recruits,
            "Make Recruits",
            (d, r) => 1f);
        _recruits = new PriorityNode(recruits, this);
        Children.Add(_recruits);

        var reinforcements = new MakeReinforcementTroopsPriority(
            "Make Reinforcements");
        _reinforcements = new PriorityNode(reinforcements, this);
        Children.Add(_reinforcements);
    }

    public override void SetWeights(float selfWeight, Regime r, Data d)
    {
        Weight = new ZeroToOne(selfWeight);
        _recruits.SetWeight(.5f);
        _reinforcements.SetWeight(.5f);
    }
}