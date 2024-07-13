
using System.Linq;

public class MilitaryCap : Flow
{
    public MilitaryCap()
    {
    }

    public override float GetNonBuildingSupply(Regime r, Data d)
    {
        return 1000f;
    }
}