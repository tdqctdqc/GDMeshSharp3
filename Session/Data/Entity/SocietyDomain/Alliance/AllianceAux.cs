using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux : EntityAux<Alliance>
{
    public EntityMultiReverseIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public RefAction<(Regime r, Alliance oldA, Alliance newA)> RegimeChangedAlliance { get; private set; }
    public AllianceAux(Domain domain, Data data) : base(domain, data)
    {
        RegimeChangedAlliance = new RefAction<(Regime r, Alliance oldA, Alliance newA)>();
        var regimeNewAlliance = new RefAction<(Regime, Alliance)>();
        RegimeChangedAlliance.Subscribe(c => regimeNewAlliance.Invoke((c.r, c.newA)));
        RegimeAlliances = new EntityMultiReverseIndexer<Alliance, Regime>(
            a => a.Members.Entities(data), regimeNewAlliance, data);
    }
}
