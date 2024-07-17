
using System.Linq;

public class Research : Flow
{
    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        return r.GetPopulation(d) / 500f
               + r.GetCells(d)
                   .Where(c => c.HasSettlement(d))
                   .Sum(c => c.GetSettlement(d).Cell.Get(d).GetPeep(d).Size)
               / 200f;
    }
}