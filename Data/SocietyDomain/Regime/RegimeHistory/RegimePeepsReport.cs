using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class RegimePeepsReport
{
    public int Tick { get; private set; }
    public int TotalPop { get; private set; }
    public int Unemployed { get; private set; }

    public static RegimePeepsReport Construct(Regime r, Data d)
    {
        return new RegimePeepsReport(d.Tick, r.GetPeeps(d).Sum(p => p.Size),
            r.GetCells(d).Sum(p => p.GetPeep(d).Employment.NumUnemployed(d)));
    }
    [SerializationConstructor] private RegimePeepsReport(int tick, int totalPop, int unemployed)
    {
        Tick = tick;
        TotalPop = totalPop;
        Unemployed = unemployed;
    }
}
