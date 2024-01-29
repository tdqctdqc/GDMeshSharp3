using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux
{
    public EntityRefColIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public RefAction<(Alliance, Regime)> AllianceAddedRegime { get; private set; }
    public RefAction<(Alliance, Regime)> AllianceRemovedRegime { get; private set; }
    public RefAction<(Alliance, Alliance)> RivalryDeclared { get; private set; }
    public RefAction<(Alliance, Alliance)> RivalryEnded { get; private set; }
    public RefAction<(Alliance, Alliance)> WarDeclared { get; private set; }
    public RefAction<(Alliance, Alliance)> WarEnded { get; private set; }
    public RefAction<Alliance> AllianceDissolved { get; private set; }
    public AllianceAux(Data data)
    {
        var membersMeta = data
            .GetEntityMeta<Alliance>()
            .GetRefColMeta<Regime>(nameof(Alliance.Members));
        RivalryDeclared = new RefAction<(Alliance, Alliance)>();
        RivalryEnded = new RefAction<(Alliance, Alliance)>();
        WarDeclared = new RefAction<(Alliance, Alliance)>();
        WarEnded = new RefAction<(Alliance, Alliance)>();
        RegimeAlliances = new EntityRefColIndexer<Alliance, Regime>(
            a => a.Members.Items(data), membersMeta, data);
        AllianceAddedRegime = new RefAction<(Alliance, Regime)>();
        membersMeta.Added.Subscribe(AllianceAddedRegime);
        AllianceRemovedRegime = new RefAction<(Alliance, Regime)>();
        membersMeta.Removed.Subscribe(AllianceRemovedRegime);
        AllianceDissolved = new RefAction<Alliance>();
        data.SubscribeForDestruction<Alliance>(n => AllianceDissolved.Invoke((Alliance)n.Entity));
    }
}
