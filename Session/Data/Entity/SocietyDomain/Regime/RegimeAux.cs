using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeAux : EntityAux<Regime>
{
    public EntityMultiIndexer<Regime, MapPolygon> Territories { get; private set; }
    public RegimeAux(Data data) : base(data)
    {
        var changedRegime = data.Planet.PolygonAux.ChangedRegime;
        Territories = new EntityMultiIndexer<Regime, MapPolygon>(
            data, 
            p => p.Regime.Entity(data),
            new RefAction[]{data.Notices.FinishedStateSync, data.Notices.GeneratedRegimes},
            changedRegime
        );
    }
}