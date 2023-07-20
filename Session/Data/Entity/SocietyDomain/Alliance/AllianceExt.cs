using System;
using System.Collections.Generic;
using System.Linq;

public static class AllianceExt
{
    public static float GetPowerScore(this Alliance a, Data data)
    {
        return a.Members.Entities(data).Sum(r => r.GetPowerScore(data));
    }

    public static float GetWeightInAlliance(this Alliance a, Regime r, Data data)
    {
        var w = r.GetPowerScore(data);
        w *= w;
        if (a.Leader.RefId == r.Id) w *= 2;
        return w;
    }
}
