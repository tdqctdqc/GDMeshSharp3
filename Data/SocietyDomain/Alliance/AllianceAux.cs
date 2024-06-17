using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux
{
    public ManyToOneIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public AllianceAux(Data data)
    {
        RegimeAlliances = ManyToOneIndexer.MakeForEntity<Alliance, Regime>(
            r => r.Members,
            data);
    }
}
