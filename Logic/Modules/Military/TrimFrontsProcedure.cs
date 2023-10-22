
using System.Collections.Generic;

public class TrimFrontsProcedure : Procedure
{
    public override void Enact(ProcedureWriteKey key)
    {
        var regimes = key.Data.GetAll<Regime>();
        foreach (var regime in regimes)
        {
            regime.Military.TrimFronts(key);
        }
    }

    public override bool Valid(Data data)
    {
        return true;
    }
}