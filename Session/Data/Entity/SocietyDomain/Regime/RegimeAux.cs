using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeAux : EntityAux<Regime>
{
    public EntityMultiIndexer<Regime, MapPolygon> Territories { get; private set; }
    public RegimeAux(Domain domain, Data data) : base(domain, data)
    {
        var changedRegime = data.Planet.PolygonAux.ChangedRegime;
        Territories = new EntityMultiIndexer<Regime, MapPolygon>(
            data, 
            p => p.Regime,
            new RefAction[]{data.Notices.FinishedStateSync, data.Notices.GeneratedRegimes},
            changedRegime
        );
    }
}