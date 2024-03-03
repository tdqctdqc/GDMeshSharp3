using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DiplomacyPolyFill : PolyFillChunkGraphic
{
    public DiplomacyPolyFill(MapChunk chunk, 
        GraphicsSegmenter segmenter, 
        Data data) 
        : base(nameof(DiplomacyPolyFill), chunk, 
            LayerOrder.PolyFill,
            segmenter, data)
    {
        
    }

    public override Color GetColor(MapPolygon poly, Data d)
    {
        if (poly.OwnerRegime.Fulfilled() == false) return Colors.Transparent;
        if (d.BaseDomain.PlayerAux.LocalPlayer == null) return Colors.Gray;
        if (d.BaseDomain.PlayerAux.LocalPlayer.Regime.IsEmpty()) return Colors.Gray;
        var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(d);
        if (poly.OwnerRegime.RefId == playerRegime.Id) return Colors.Green;
        var playerAlliance = playerRegime.GetAlliance(d);
        var polyAlliance = poly.OwnerRegime.Entity(d).GetAlliance(d);
        if (playerAlliance.Members.RefIds.Contains(poly.OwnerRegime.RefId)) 
            return Colors.SkyBlue;
        if (playerAlliance.IsAtWar(polyAlliance, d)) 
            return Colors.Red;
        if (playerAlliance.IsRivals(polyAlliance, d)) 
            return Colors.Orange;
        return Colors.Gray;
    }
}
