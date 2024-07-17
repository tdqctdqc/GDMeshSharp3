using System.Linq;

public class Labor : Flow
{
    public Labor()
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        return r.GetCells(d).Sum(c => c.GetPeep(d).Size);
    }
}