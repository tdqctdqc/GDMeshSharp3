
public class MilitaryBudgetBranch
    : BudgetBranch
{
    private PriorityNode _recruitBuildings, _reinforcements, _reserve;
    public MilitaryBudgetBranch(Data d)
    {
        var recruits = new MakeProductionBuildingsPriority(
            d.Models.Items.Recruits,
            "Make Recruit Buildings",
            (d, r) => 1f);
        _recruitBuildings = new PriorityNode(recruits, this);
        Children.Add(_recruitBuildings);

        var reinforcements = new MakeReinforcementTroopsPriority(
            "Make Reinforcement Troops");
        _reinforcements = new PriorityNode(reinforcements, this);
        Children.Add(_reinforcements);


        var reserve = new MakeReserveTroopsPriority("Make Reserve Troops");
        _reserve = new PriorityNode(reserve, this);
        Children.Add(_reserve);
    }

    public override void SetWeights(float selfWeight, Regime r, Data d)
    {
        Weight = new ZeroToOne(selfWeight);
        _recruitBuildings.SetWeight(.15f);
        _reinforcements.SetWeight(.5f);
        _reserve.SetWeight(.35f);
    }
}