using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceFillNode : PolyFillChunkGraphic
{
    public AllianceFillNode(MapChunk chunk, Data data) 
        : base(nameof(AllianceFillNode), chunk, GetColor, data)
    {
        
    }

    private static Color GetColor(MapPolygon p, Data d)
    {
        if (p.Regime.Fulfilled() == false) return Colors.Transparent;
        if (d.BaseDomain.PlayerAux.LocalPlayer == null) return Colors.Gray;
        if (d.BaseDomain.PlayerAux.LocalPlayer.Regime.Empty())
        {
            GD.Print("empty player regime");
            return Colors.Gray;
        }
        var playerRegime = d.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(d);
        if (p.Regime.RefId == playerRegime.Id) return Colors.SkyBlue;
        var playerAlliance = playerRegime.GetAlliance(d);
        var polyAlliance = p.Regime.Entity(d).GetAlliance(d);
        if (playerAlliance.Members.RefIds.Contains(p.Regime.RefId)) 
            return Colors.Green.GetPeriodicShade(p.Regime.RefId);
        if (playerAlliance.AtWar.Contains(polyAlliance)) 
            return Colors.Red;
        if (playerAlliance.Rivals.Contains(polyAlliance)) 
            return Colors.Orange;
        return Colors.Gray;
    }
}
