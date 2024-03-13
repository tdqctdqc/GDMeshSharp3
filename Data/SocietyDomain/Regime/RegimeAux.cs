using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeAux
{
    public MultiIndexer<Regime, MapPolygon> Territories { get; private set; }
    public RegimeAux(Data data)
    {
        var changedRegime = data.Notices.Political.ChangedOwnerRegime;
        Territories = MultiIndexer.MakeForEntity<Regime, MapPolygon>(
            p => p.OwnerRegime.Get(data),
            data
        );
        Territories.RegisterChanged(changedRegime);
        Territories.RegisterReCalc(data.Notices.FinishedStateSync);
        Territories.RegisterReCalc(data.Notices.Gen.GeneratedRegimes);
    }
}