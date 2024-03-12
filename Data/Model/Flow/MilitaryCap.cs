
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
}