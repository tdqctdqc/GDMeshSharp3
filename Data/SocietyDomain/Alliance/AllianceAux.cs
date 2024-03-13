using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux
{
    public ERefColIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public AllianceAux(Data data)
    {
        var membersMeta = data
            .GetEntityMeta<Alliance>()
            .GetRefColMeta<Regime>(nameof(Alliance.Members));
        RegimeAlliances = new ERefColIndexer<Alliance, Regime>(
            a => a.Members.Items(data), membersMeta, data);
        membersMeta.Added.Subscribe(data.Notices.Political.AllianceAddedRegime);
        membersMeta.Removed.Subscribe(data.Notices.Political.AllianceRemovedRegime);
        data.SubscribeForDestruction<Alliance>(n => data.Notices.Political.AllianceDissolved.Invoke((Alliance)n.Entity));
    }
}
