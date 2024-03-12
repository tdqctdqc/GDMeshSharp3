using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeAux
{
    public EntityMultiIndexer<Regime, MapPolygon> Territories { get; private set; }
    public RegimeAux(Data data)
    {
        var changedRegime = data.Planet.PolygonAux.ChangedOwnerRegime;
        Territories = new EntityMultiIndexer<Regime, MapPolygon>(
            data, 
            p => p.OwnerRegime.Get(data),
            new RefAction[]{data.Notices.FinishedStateSync, data.Notices.GeneratedRegimes},
            changedRegime
        );
    }
}