using System.Linq;
using Godot;
using MessagePack;

public class MakeRecruitmentBuildingsPriority
    : MakeProductionBuildingsPriority
{
    public static MakeRecruitmentBuildingsPriority Construct(Data d)
    {
        return new MakeRecruitmentBuildingsPriority(
            d.Models.Items.Recruits.MakeRef<IModel>(),
            "Make Recruitment Buildings");
    }
    [SerializationConstructor] private MakeRecruitmentBuildingsPriority(ModelRef<IModel> model, 
        string name) : base(model, name)
    {
    }

    public override float GetWeight(Regime r, Data d)
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
    }
}