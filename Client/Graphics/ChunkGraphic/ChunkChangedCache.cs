using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkChangedCache
{
    public ChunkChangeListener<int> BuildingsChanged { get; private set; }
    public ChunkChangeListener<int> RoadsChanged { get; private set; }
    // public ChunkChangeListener TerrainChanged { get; private set; }
    public ChunkChangeListener<int> PolyRegimeChanged { get; private set; }
    public ChunkChangeListener<Construction> ConstructionsChanged { get; private set; }
    public ChunkChangeListener<int> SettlementTierChanged { get; private set; }
    
    public ChunkChangedCache(Data d)
    {
        BuildingsChanged = new ChunkChangeListener<int>(d);
        BuildingsChanged.ListenForPolyEntity<MapBuilding, int>(d,
            mb => mb.Id, mb => mb.Position.Poly(d));

        RoadsChanged = new ChunkChangeListener<int>(d);
        RoadsChanged.ListenForMultiPolyEntity<RoadSegment, int>(
            d,
            e => e.Edge.Entity(d).Id,
            e => new MapPolygon[]{e.Edge.Entity(d).HighPoly.Entity(d), e.Edge.Entity(d).LowPoly.Entity(d)});
        
        var px = d.Planet.PolygonAux;
        var changedRegime = d.Planet.PolygonAux.ChangedRegime;
        
        PolyRegimeChanged = new ChunkChangeListener<int>(d);
        PolyRegimeChanged.ListenForChange<int, MapPolygon, Regime>(
            d, changedRegime, p => p, p => p.Id);

        ConstructionsChanged = new ChunkChangeListener<Construction>(d);
        ConstructionsChanged.Listen<Construction, Construction>(
            d, 
            v => v.Pos.Poly(d),
            v => v,
            d.Notices.StartedConstruction,
            d.Notices.EndedConstruction
        );
        
        SettlementTierChanged = new ChunkChangeListener<int>(d);
        SettlementTierChanged.ListenForChange<int, Settlement, SettlementTier>(
            d,
            d.Infrastructure.SettlementAux.ChangedTier,
            e => e.Poly.Entity(d),
            s => s.Poly.RefId
        );
    }
}
