using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class MapPolygonEdge : Entity
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public float MoistureFlow { get; protected set; }
    public PolyBorderChain LowSegsRel() => LowPoly.Entity().NeighborBorders[HighPoly.RefId];
    public PolyBorderChain HighSegsRel() => HighPoly.Entity().NeighborBorders[LowPoly.RefId];
    public EntityRef<MapPolygon> LowPoly { get; protected set; }
    public EntityRef<MapPolygon> HighPoly { get; protected set; }
    public Dictionary<byte, byte> HiToLoTriPaths { get; private set; }
    public Dictionary<byte, byte> LoToHiTriPaths { get; private set; }
    public EntityRef<MapPolyNexus> HiNexus { get; private set; }
    public EntityRef<MapPolyNexus> LoNexus { get; private set; }
    [SerializationConstructor] private MapPolygonEdge(int id, float moistureFlow, 
        EntityRef<MapPolygon> lowPoly, EntityRef<MapPolygon> highPoly, Dictionary<byte, byte> hiToLoTriPaths,
        Dictionary<byte, byte> loToHiTriPaths, EntityRef<MapPolyNexus> loNexus, EntityRef<MapPolyNexus> hiNexus) 
        : base(id)
    {
        HiToLoTriPaths = hiToLoTriPaths;
        LoToHiTriPaths = loToHiTriPaths;
        MoistureFlow = moistureFlow;
        LowPoly = lowPoly;
        HighPoly = highPoly;
        LoNexus = loNexus;
        HiNexus = hiNexus;
    }
    public static MapPolygonEdge Create(PolyBorderChain hiChain, PolyBorderChain lowChain, GenWriteKey key)
    {
        var lowId = lowChain.Native;
        var highId = hiChain.Native;
        
        lowId.Entity().AddNeighbor(highId.Entity(), lowChain, key);
        highId.Entity().AddNeighbor(lowId.Entity(), hiChain, key);
        var b = new MapPolygonEdge(
            key.IdDispenser.GetID(), 0f, lowId, highId,
            new Dictionary<byte, byte>(), new Dictionary<byte, byte>(),
            null, null);
        key.Create(b);
        return b;
    }
    
    public static PolyBorderChain ConstructBorderChain(MapPolygon native, MapPolygon foreign, 
        List<LineSegment> segmentsRel, Data data)
    {
        
        return PolyBorderChain.Construct(native, foreign, segmentsRel);
    }
    private List<LineSegment> RelativizeSegments(List<LineSegment> abs, MapPolygon poly, Data data)
    {
        var oldSegs = this.GetSegsRel(poly).Segments;
        var oldFrom = oldSegs[0].From;
        var oldTo = oldSegs[oldSegs.Count - 1].To;
        
        var absFirstRel = poly.GetOffsetTo(abs[0].From, data);
        var absLastRel = poly.GetOffsetTo(abs[abs.Count - 1].To, data);

        List<LineSegment> newSegs;
        if (absFirstRel == oldFrom && absLastRel == oldTo)
        {
            newSegs = abs
                .Select(s => 
                    new LineSegment(poly.GetOffsetTo(s.From, data).RoundTo2Digits(), 
                        poly.GetOffsetTo(s.To, data).RoundTo2Digits()))
                .ToList();
        }
        else if (absLastRel == oldFrom && absFirstRel == oldTo)
        {
            newSegs = abs
                .Select(s => 
                    new LineSegment(poly.GetOffsetTo(s.To, data).RoundTo2Digits(), 
                        poly.GetOffsetTo(s.From, data).RoundTo2Digits()))
                .Reverse()
                .ToList();
        }
        else throw new Exception();
        
        return newSegs;
    }
    public void ReplaceMiddlePoints(List<LineSegment> newSegmentsAbs, GenWriteKey key)
    {
        var hiPoly = HighPoly.Entity();
        var loPoly = LowPoly.Entity();
        var highBorderSegs = RelativizeSegments(newSegmentsAbs, hiPoly, key.Data);
        var lowBorderSegs = RelativizeSegments(newSegmentsAbs, loPoly, key.Data);
        
        var loChain = PolyBorderChain.Construct(loPoly, hiPoly, 
            lowBorderSegs);
        var hiChain = PolyBorderChain.Construct(hiPoly, loPoly, 
            highBorderSegs);
        
        hiPoly.SetNeighborBorder(loPoly, hiChain, key);
        loPoly.SetNeighborBorder(hiPoly, loChain, key);
        
        key.Data.Planet.PolygonAux.AuxDatas.Dic[hiPoly].MarkStale(key);
        key.Data.Planet.PolygonAux.AuxDatas.Dic[loPoly].MarkStale(key);
    }
    public void ReplacePoints(MapPolygon poly, List<LineSegment> newSegsRel, GenWriteKey key)
    {
        var hiPoly = HighPoly.Entity();
        var loPoly = LowPoly.Entity();

        var otherPoly = poly == hiPoly ? loPoly : hiPoly;
        var newChain = PolyBorderChain.Construct(poly, otherPoly, newSegsRel);
        
        poly.SetNeighborBorder(otherPoly, newChain, key);
        
        key.Data.Planet.PolygonAux.AuxDatas.Dic[hiPoly].MarkStale(key);
        key.Data.Planet.PolygonAux.AuxDatas.Dic[loPoly].MarkStale(key);
    }
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }

    public void SetNexi(MapPolyNexus n1, MapPolyNexus n2, GenWriteKey key)
    {
        HiNexus = n1.MakeRef();
        LoNexus = n2.MakeRef();
    }
}

