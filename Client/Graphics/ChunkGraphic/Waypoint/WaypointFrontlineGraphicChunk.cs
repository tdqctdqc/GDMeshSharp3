
using System;
using System.Linq;
using Godot;

public partial class WaypointFrontlineGraphicChunk : WaypointGraphicChunk
{
    public WaypointFrontlineGraphicChunk(MapChunk chunk, Data d) : base(chunk, d)
    {
    }

    public override (Color inner, Color border) GetColor(Waypoint wp, Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var player = data.BaseDomain.PlayerAux
            .LocalPlayer;
        var transparent = (Colors.Transparent, Colors.Transparent);
        if (player.Regime.Empty()) return transparent;
        var alliance = player.Regime.Entity(data).GetAlliance(data);
        var frontlineHash = data.HostLogicData.AllianceAis[alliance].MilitaryAi.FrontlineHash;
        if (frontlineHash.Contains(wp) == false) return transparent;
        
        if (forceBalances.TryGetValue(wp, out var forceBalance) == false
            || forceBalance.ContainsKey(alliance) == false)
        {
            return transparent;
        }
        else if (forceBalance.GetAllianceWithForceSupremacy(data) == alliance)
        {
            return (Colors.Green, Colors.Black);
        }
        else if (forceBalance.GetAllianceWithForceSuperiority(data) == alliance)
        {
            return (Colors.Yellow, Colors.Black);
        }
        else if (forceBalance.GetControllingAlliances().Contains(alliance))
        {
            return (Colors.Orange, Colors.Black);
        }
        else return (Colors.Red, Colors.Black);
    }
    
    public static ChunkGraphicLayer<WaypointGraphicChunk> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<WaypointGraphicChunk>("Waypoint Frontline", segmenter, 
            c => new WaypointFrontlineGraphicChunk(c, d), d);
        Action update = () =>
        {
            foreach (var wpChunk in l.ByChunkCoords.Values)
            {
                wpChunk.ClearChildren();
                wpChunk.Draw(wpChunk.Chunk, d);
            }
        };
        d.Notices.FinishedTurnStartCalc.Subscribe(update);
        d.BaseDomain.PlayerAux.PlayerChangedRegime
            .Subscribe(a => update());
        return l;
    }
}