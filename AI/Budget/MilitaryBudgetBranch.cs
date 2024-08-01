
using System.Linq;
using Godot;

public class MilitaryBudgetBranch
    : BudgetBranch
{
    private PriorityNode _recruitBuildings, _reinforcements, 
        _reserve, _units, _upgrade;
    public MilitaryBudgetBranch(Regime r, BudgetBranch parent, Data d)
        : base("Military")
    {
        Parent = parent;
        var recruits = new MakeProductionBuildingsPriority(
            d.Models.Items.Recruits,
            r,
            "Make Recruit Buildings");
        _recruitBuildings = new PriorityNode(recruits, this,
            (r, d) =>
            {
                var score = 0f;
                var recruit = d.Models.Items.Recruits;

                var units = r.GetUnits(d)?.ToArray();
                if (units is null || units.Length == 0) return 1f;
                
                var numRecruits = r.GetUnits(d)
                    .Sum(u => u.Troops.GetEnumModel(d)
                        .Sum(kvp => kvp.Key.Makeable.BuildCosts.Get(recruit) * kvp.Value));
                var numRecruitsAuthorized = r.GetUnits(d)
                    .Sum(u => u.Template.Get(d).Troops.GetEnumModel(d)
                        .Sum(kvp => 
                            r.Military.GetBestTroopOfType(kvp.Key, d)
                                .Makeable.BuildCosts.Get(recruit) * kvp.Value));
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
            r);
        _reinforcements = new PriorityNode(reinforcements, this,
            (r, d) =>
            {
                var units = r.GetUnits(d);
                var str = units.Sum(u => u.GetPowerPoints(d));
                var authorized = units.Sum(u => u.Template.Get(d).Troops.GetEnumModel(d)
                    .Sum(kvp => 
                        r.Military.GetBestTroopOfType(kvp.Key, d)
                            .GetPowerPoints() * kvp.Value));
                if (authorized == 0f) return 0f;
                return 3f * (1f - str / authorized);
            });
        Children.Add(_reinforcements);


        var reserve = new MakeReserveTroopsPriority(r);
        _reserve = new PriorityNode(reserve, this,
            (d, r) => 1f);
        Children.Add(_reserve);

        var units = new MakeUnitPriority(r, d);
        _units = new PriorityNode(units, this,
            (r, d) =>
            {
                var baseWeight = 5f;
                var templates = d.HostLogicData.RegimeAis[r].Military.Templates;
                var desired = d.HostLogicData.RegimeAis[r].Military.ForceComposition.DesiredAmounts;
                var allUnits = r.GetUnits(d)?.ToArray();
                if (allUnits is null || allUnits.Count() == 0) return baseWeight;
                var unitsByMeta = r.GetUnits(d)
                    .SortBy(u => u.Template.Get(d).GetMetaTemplate(d));
                var needed = desired.ToDictionary(kvp => kvp.Key,
                    kvp => unitsByMeta.TryGetValue(kvp.Key, out var list)
                        ? Mathf.Max(0, kvp.Value - list.Count)
                        : kvp.Value);
                
                return baseWeight * needed.Sum(kvp => kvp.Value)
                       / allUnits.Count();
                
            });
        Children.Add(_units);


        var upgrade = new UpgradeTroopsPriority();
        _upgrade = new PriorityNode(upgrade,
            this, (r, d) =>
            {
                var troops = r.GetAllTroopAmounts(d);
                var activeTroopTypes = troops.Select(kvp => kvp.Key.TroopType).ToHashSet();
                var totalPp = troops.Sum(kvp => kvp.Key.GetPowerPoints() * kvp.Value);
                if (totalPp == 0f) return 0f;
                var upgradePotential = 0f;
                foreach (var (troop, value) in troops)
                {
                    var best = r.Military.GetBestTroopOfType(troop.TroopType, d);
                    if (best != troop)
                    {
                        upgradePotential += (best.GetPowerPoints() - troop.GetPowerPoints()) * value;
                    }
                }
                var score = Mathf.Min(10f, 100f * (upgradePotential / totalPp));
                return score;
            });
        Children.Add(_upgrade);

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