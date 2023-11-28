using System;
using System.Collections.Generic;
using System.Linq;

public class AllianceAux
{
    public EntityRefColIndexer<Alliance, Regime> RegimeAlliances { get; private set; }
    public RefAction<(Alliance, Regime)> AllianceAddedRegime { get; private set; }
    public RefAction<(Alliance, Regime)> AllianceRemovedRegime { get; private set; }
    
    
    public RefAction<(Alliance, Alliance)> AllianceRelationChanged { get; private set; }
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
        var rivalsMeta = data
            .GetEntityMeta<Alliance>()
            .GetRefColMeta<Alliance>(nameof(Alliance.Rivals));
        var atWarMeta = data
            .GetEntityMeta<Alliance>()
            .GetRefColMeta<Alliance>(nameof(Alliance.AtWar));
        AllianceRelationChanged = new RefAction<(Alliance, Alliance)>();
        RivalryDeclared = new RefAction<(Alliance, Alliance)>();
        RivalryEnded = new RefAction<(Alliance, Alliance)>();
        rivalsMeta.Added.Subscribe(RivalryDeclared);
        rivalsMeta.Removed.Subscribe(RivalryEnded);
        RivalryDeclared.Subscribe(AllianceRelationChanged);
        RivalryEnded.Subscribe(AllianceRelationChanged);

        WarDeclared = new RefAction<(Alliance, Alliance)>();
        WarEnded = new RefAction<(Alliance, Alliance)>();
        atWarMeta.Added.Subscribe(WarDeclared);
        atWarMeta.Removed.Subscribe(WarEnded);
        WarDeclared.Subscribe(AllianceRelationChanged);
        WarEnded.Subscribe(AllianceRelationChanged);
        
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
