
using System.Linq;

public class MilitaryCap : Flow
{
    public MilitaryCap() : base(nameof(MilitaryCap))
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        return 1000f;
    }

    public override float GetConsumption(Regime r, Data d)
    {
        return r.GetUnits(d)
            .Sum(u => u.Template.Entity(d)
                .TroopCounts.GetEnumerableModel(d)
                .Sum(v => v.Key.MilitaryCapCost));
    }
}