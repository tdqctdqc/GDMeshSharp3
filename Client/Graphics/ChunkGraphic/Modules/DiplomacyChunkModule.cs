using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class DiplomacyChunkModule : MapChunkGraphicModule
{
    private DiplomacyPolyFill _diplomacyFill;
    private AllianceBordersNode _allianceBorder;
    
    public DiplomacyChunkModule(MapChunk chunk, Data data) : base(chunk, nameof(DiplomacyChunkModule))
    {
        _diplomacyFill = new DiplomacyPolyFill(chunk, data);
        AddNode(_diplomacyFill);

        _allianceBorder = new AllianceBordersNode(chunk, data, true);
        AddNode(_allianceBorder);
        
        data.BaseDomain.PlayerAux.PlayerChangedRegime
            .SubscribeForNode(n => HandlePlayerRegimeChange(n, data), this);
        
        data.Society.AllianceAux.AllianceRelationChanged
            .SubscribeForNode(n => HandleAllianceRelationChange(n.Item1, n.Item2, data), this);
        
        data.Society.AllianceAux.AllianceAddedRegime
            .SubscribeForNode(n => HandleRegimeAllianceChange(n.Item2, n.Item1, data), this);
        // data.Society.AllianceAux.AllianceRemovedRegime
        //     .SubscribeForNode(n => HandleRegimeAllianceChange(n.Item2, n.Item1, data), this);
    }
    
    public static ChunkGraphicLayer<DiplomacyChunkModule> GetLayer(Data d, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<DiplomacyChunkModule>(
            "Diplomacy",
            segmenter, 
            c => new DiplomacyChunkModule(c, d), 
            d);
        l.AddTransparencySetting(m => m._diplomacyFill);
        l.RegisterForChunkNotice(d.Planet.PolygonAux.ChangedRegime,
            r => r.Entity.GetChunkAndNeighboringChunks(d),
            (n, m) => { m.HandlePolygonRegimeChange(n, d); });
        return l;
    }

    private void HandleRegimeAllianceChange(Regime regime, Alliance newA, Data d)
    {
        var relevant = Chunk.Polys.Union(Chunk.Bordering)
            .Where(p => p.Regime.RefId == regime.Id
            || (p.Regime.Fulfilled() && p.Regime.Entity(d).GetAlliance(d) == newA));
        if (relevant.Count() == 0) return;
        // GD.Print($"relevant regime alliance change " +
        //          $"{regime.Name} {regime.Id} had relation change w alliance of " +
        //          $"{newA.Leader.Entity(d).Name} {newA.Id}");
        foreach (var p in relevant)
        {
            if(Chunk.Polys.Contains(p)) _diplomacyFill.Updates.Add(p);
        }
        _allianceBorder.QueueChangeAll();

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
        // GD.Print($"relevant alliance relation change between " +
        //          $"{a1.Leader.Entity(d).Name} {a1.Id} " +
        //          $"and {a2.Leader.Entity(d).Name} {a2.Id}");
        _diplomacyFill.Updates.AddRange(polys);
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
                _diplomacyFill.Updates.AddRange(relevant);
                updated = true;
            }
        }
        if (newRegime != null)
        {
            var newAlliance = newRegime.GetAlliance(d);
            var relevant = Chunk.Polys.Where(p => isRelevant(p, newAlliance));
            if (relevant.Count() > 0)
            {
                _diplomacyFill.Updates.AddRange(relevant);
                updated = true;
            }
        }
        
        if (updated == false) return;
        // GD.Print("relevant player regime change");
        _allianceBorder.QueueChangeAll();

        _diplomacyFill.Update(d, Game.I.Client.GetComponent<MapGraphics>().UpdateQueue);

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
        // GD.Print("relevant polygon regime change");
        _diplomacyFill.Updates.Add(notice.Entity);
        _allianceBorder.QueueChangeAll();
    }
}
