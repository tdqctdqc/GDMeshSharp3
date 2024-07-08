
using System.Linq;
using Godot;

public class MilitaryBudgetBranch
    : BudgetBranch
{
    private PriorityNode _recruitBuildings, _reinforcements, _reserve;
    public MilitaryBudgetBranch(BudgetBranch parent, Data d)
        : base("Military")
    {
        Parent = parent;
        var recruits = new MakeProductionBuildingsPriority(
            d.Models.Items.Recruits,
            "Make Recruit Buildings");
        _recruitBuildings = new PriorityNode(recruits, this,
            (r, d) =>
            {
                var score = 0f;
                var recruit = d.Models.Items.Recruits;
                var numRecruits = r.GetUnits(d)
                    .Sum(u => u.Troops.GetEnumModel(d)
                        .Sum(kvp => kvp.Key.Makeable.BuildCosts.Get(recruit) * kvp.Value));
                var numRecruitsAuthorized = r.GetUnits(d)
                    .Sum(u => u.Template.Get(d).Troops.GetEnumModel(d)
                        .Sum(kvp => kvp.Key.Makeable.BuildCosts.Get(recruit) * kvp.Value));
                if (numRecruitsAuthorized > 0f)
                {
                    score += .1f * (1f - numRecruits / numRecruitsAuthorized);
                }
                var lastProd = r.Stock.Produced.Get(recruit);
                score += .1f * Mathf.Clamp(1f - lastProd * 10f / numRecruitsAuthorized, 0f, 1f);
                return score;
            });
        Children.Add(_recruitBuildings);

        var reinforcements = new MakeReinforcementTroopsPriority(
            "Make Reinforcement Troops");
        _reinforcements = new PriorityNode(reinforcements, this,
            (r, d) =>
            {
                var units = r.GetUnits(d);
                var str = units.Sum(u => u.GetPowerPoints(d));
                var authorized = units.Sum(u => u.Template.Get(d).GetPowerPoints(d));
                if (authorized == 0f) return 0f;
                return 3f * (1f - str / authorized);
            });
        Children.Add(_reinforcements);


        var reserve = new MakeReserveTroopsPriority("Make Reserve Troops");
        _reserve = new PriorityNode(reserve, this,
            (d, r) => 1f);
        Children.Add(_reserve);
    }

    protected override float GetWeight(Regime r, Data d)
    {
        var alliance = r.GetAlliance(d);
        var allianceStr = alliance.GetPowerScore(d);
        var rivalStr = alliance.GetRivals(d)
            .Sum(rival =>
            {
                var mult = rival.IsAtWar(alliance, d)
                    ? 2f
                    : 1f;
                return rival.GetPowerScore(d) * mult;
            });
        var score = rivalStr * 1.5f / allianceStr;
        score = Mathf.Max(score, .5f);
        return score;
    }
}