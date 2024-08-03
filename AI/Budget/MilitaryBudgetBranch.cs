
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MilitaryBudgetBranch
    : BudgetBranch
{
    public static MilitaryBudgetBranch Construct(Regime r,
        BudgetRoot root, Data d)
    {
        var b = new MilitaryBudgetBranch(new List<IBudgetNode>(),
            0f, "Military");

        var recruitBuildingsPriority = MakeRecruitmentBuildingsPriority.Construct(d);
        var recruitBuildingsNode = new PriorityNode(recruitBuildingsPriority);
        root.SetParent(recruitBuildingsNode, b);
        
        var reinforcementsPriority = new MakeReinforcementTroopsPriority(
            "Make reinforcements");
        var reinforcementsNode = new PriorityNode(reinforcementsPriority);
        root.SetParent(reinforcementsNode, b);


        var reservePriority = new MakeReserveTroopsPriority();
        var reserveNode = new PriorityNode(reservePriority);
        root.SetParent(reserveNode, b);

        var unitsPriority = new MakeUnitPriority();
        var unitsNode = new PriorityNode(unitsPriority);
        root.SetParent(unitsNode, b);

        var upgradePriority = new UpgradeTroopsPriority();
        var upgradeNode = new PriorityNode(upgradePriority);
        root.SetParent(upgradeNode, b);

        return b;
    }
    public MilitaryBudgetBranch(List<IBudgetNode> children, float weight, string name) : base(children, weight, name)
    {
    }

    protected override float GetWeight(Regime r, BudgetRoot root, Data d)
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