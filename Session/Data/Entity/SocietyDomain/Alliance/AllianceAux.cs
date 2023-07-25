using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux
{
    public EntityRefColIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public AllianceAux(Data data)
    {
        var membersMeta = data
            .GetEntityMeta<Alliance>()
            .GetRefColMeta<Regime>(nameof(Alliance.Members));

        RegimeAlliances = new EntityRefColIndexer<Alliance, Regime>(
            a => a.Members.Items(data), membersMeta, data);
    }
}
