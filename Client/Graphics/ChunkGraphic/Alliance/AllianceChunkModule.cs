using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class AllianceChunkModule : MapChunkGraphicModule
{
    private AllianceFillNode _fill;
    private AllianceBorderNode _allianceBorder;
    private BorderChunkNode _regimeBorder;
    
    public AllianceChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(AllianceChunkModule))
    {
        _fill = new AllianceFillNode(chunk, data);
        AddNode(_fill);

        data.BaseDomain.PlayerAux.PlayerChangedRegime
            .SubscribeForNode(n => HandlePlayerRegimeChange(n, data), this);
        
        data.Society.AllianceAux.RivalryDeclared
            .SubscribeForNode(n => HandleAllianceRelationChange(n.Item1, n.Item2, data), this);
        data.Society.AllianceAux.RivalryEnded
            .SubscribeForNode(n => HandleAllianceRelationChange(n.Item1, n.Item2, data), this);
        data.Society.AllianceAux.WarDeclared
            .SubscribeForNode(n => HandleAllianceRelationChange(n.Item1, n.Item2, data), this);
        data.Society.AllianceAux.WarEnded
            .SubscribeForNode(n => HandleAllianceRelationChange(n.Item1, n.Item2, data), this);
        
        data.Society.AllianceAux.AllianceAddedRegime
            .SubscribeForNode(n => HandleRegimeAllianceChange(n.Item2, n.Item1, data), this);
        data.Society.AllianceAux.AllianceRemovedRegime
            .SubscribeForNode(n => HandleRegimeAllianceChange(n.Item2, n.Item1, data), this);
        
        _allianceBorder = new AllianceBorderNode(chunk, 30f, data);
        AddNode(_allianceBorder);
        
        _regimeBorder = new BorderChunkNode(nameof(Alliance), chunk,
            (p, n) => p.Regime.RefId == n.Regime.RefId,
            p => p.Regime.Fulfilled() 
                ? p.Regime.Entity(data).GetAlliance(data).Leader.Entity(data).PrimaryColor 
                : Colors.Transparent,
            5f, data);
        AddNode(_regimeBorder);
    }
    
    public static ChunkGraphicLayer<AllianceChunkModule> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<AllianceChunkModule>(
            "Alliances",
            segmenter, 
            c => new AllianceChunkModule(c, d), 
            d);
        l.RegisterForChunkNotice(d.Planet.PolygonAux.ChangedRegime,
            r => r.Entity.GetChunk(d),
            (n, m) => { m.HandlePolygonRegimeChange(n, d); });
        return l;
    }

    private void HandleRegimeAllianceChange(Regime regime, Alliance newA, Data d)
    {
        var polys = Chunk.Polys.Where(p => p.Regime.RefId == regime.Id);
        if (polys.Count() == 0) return;
        GD.Print($"relevant regime alliance change " +
                 $"{regime.Name} {regime.Id} had relation change w alliance of " +
                 $"{newA.Leader.Entity(d).Name} {newA.Id}");
        
        _fill.Updates.AddRange(polys);
    }
    private void HandleAllianceRelationChange(Alliance a1, Alliance a2, Data d)
    {
        var player = d.BaseDomain.PlayerAux.LocalPlayer;
        if (player.Regime.Fulfilled() == false) return;
        var playerAlliance = player.Regime.Entity(d).GetAlliance(d);
        if (a1 != playerAlliance && a2 != playerAlliance) return;
        var polys = Chunk.Polys.Where(p => a1.Members.RefIds.Contains(p.Regime.RefId)
                                           || a2.Members.RefIds.Contains(p.Regime.RefId));
        if (polys.Count() == 0) return;
        GD.Print($"relevant alliance relation change between " +
                 $"{a1.Leader.Entity(d).Name} {a1.Id} " +
                 $"and {a2.Leader.Entity(d).Name} {a2.Id}");
        _fill.Updates.AddRange(polys);
    }
    private void HandlePlayerRegimeChange(ValChangeNotice<Player, Regime> n, Data d)
    {
        var player = n.Entity;
        if (player != d.BaseDomain.PlayerAux.LocalPlayer) return;
        
        var oldRegime = n.OldVal;
        var newRegime = n.NewVal;
        bool updated = false;
        if (oldRegime != null)
        {
            var oldAlliance = oldRegime.GetAlliance(d);
            var relevant = Chunk.Polys.Where(p => isRelevant(p, oldAlliance));
            if (relevant.Count() > 0)
            {
                _fill.Updates.AddRange(relevant);
                updated = true;
            }
        }
        if (newRegime != null)
        {
            var newAlliance = newRegime.GetAlliance(d);
            var relevant = Chunk.Polys.Where(p => isRelevant(p, newAlliance));
            if (relevant.Count() > 0)
            {
                _fill.Updates.AddRange(relevant);
                updated = true;
            }
        }

        if (updated == false) return;
        GD.Print("relevant player regime change");

        _fill.Update(d);

        bool isRelevant(MapPolygon p, Alliance alliance)
        {
            if (p.Regime.Fulfilled() == false) return false;
            var r = p.Regime.Entity(d);
            var a = r.GetAlliance(d);
            return alliance == a
                   || alliance.Rivals.Contains(a)
                   || alliance.AtWar.Contains(a);
        }
    }
    private void HandlePolygonRegimeChange(ValChangeNotice<MapPolygon, Regime> notice, Data data)
    {
        GD.Print("relevant polygon regime change");
        _fill.Updates.Add(notice.Entity);
        _allianceBorder.QueueChangeAround(notice.Entity, data);
        _regimeBorder.QueueChangeAround(notice.Entity, data);
    }
}
