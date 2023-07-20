using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux : EntityAux<Alliance>
{
    public EntityRefColIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public AllianceAux(Domain domain, Data data) : base(domain, data)
    {
        var membersMeta = Game.I.Serializer.GetEntityMeta<Alliance>()
            .GetRefColMeta<Regime>(nameof(Alliance.Members));
            
        RegimeAlliances = new EntityRefColIndexer<Alliance, Regime>(
            a => a.Members.Entities(data), membersMeta, data);
    }
}