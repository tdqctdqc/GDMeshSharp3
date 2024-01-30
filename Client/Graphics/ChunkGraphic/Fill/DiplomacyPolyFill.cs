using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DiplomacyPolyFill : PolyFillChunkGraphic
{
    public DiplomacyPolyFill(MapChunk chunk, Data data) 
        : base(nameof(DiplomacyPolyFill), chunk, GetColor, data)
    {
        
    }

    private static Color GetColor(MapPolygon p, Data d)
    {
        if (p.OwnerRegime.Fulfilled() == false) return Colors.Transparent;
        if (d.BaseDomain.PlayerAux.LocalPlayer == null) return Colors.Gray;
        if (d.BaseDomain.PlayerAux.LocalPlayer.Regime.IsEmpty()) return Colors.Gray;
        var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(d);
        if (p.OwnerRegime.RefId == playerRegime.Id) return Colors.Green;
        var playerAlliance = playerRegime.GetAlliance(d);
        var polyAlliance = p.OwnerRegime.Entity(d).GetAlliance(d);
        if (playerAlliance.Members.RefIds.Contains(p.OwnerRegime.RefId)) 
            return Colors.SkyBlue;
        if (playerAlliance.IsAtWar(polyAlliance, d)) 
            return Colors.Red;
        if (playerAlliance.IsRivals(polyAlliance, d)) 
            return Colors.Orange;
        return Colors.Gray;
    }
}
