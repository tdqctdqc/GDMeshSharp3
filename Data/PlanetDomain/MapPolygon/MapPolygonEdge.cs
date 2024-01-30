using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


public class MapPolygonEdge : Entity
{
    public float MoistureFlow { get; protected set; }
    public PolyBorderChain LowSegsRel(Data data) => LowPoly.Entity(data).NeighborBorders[HighPoly.RefId];
    public PolyBorderChain HighSegsRel(Data data) => HighPoly.Entity(data).NeighborBorders[LowPoly.RefId];
    public ERef<MapPolygon> LowPoly { get; protected set; }
    public ERef<MapPolygon> HighPoly { get; protected set; }
    public Dictionary<byte, byte> HiToLoTriPaths { get; private set; }
    public Dictionary<byte, byte> LoToHiTriPaths { get; private set; }
    public ERef<MapPolyNexus> HiNexus { get; private set; }
    public ERef<MapPolyNexus> LoNexus { get; private set; }
    [SerializationConstructor] private MapPolygonEdge(int id, float moistureFlow, 
        ERef<MapPolygon> lowPoly, ERef<MapPolygon> highPoly, Dictionary<byte, byte> hiToLoTriPaths,
        Dictionary<byte, byte> loToHiTriPaths, ERef<MapPolyNexus> loNexus, ERef<MapPolyNexus> hiNexus) 
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
        
        lowId.Entity(key.Data).AddNeighbor(highId.Entity(key.Data), lowChain, key);
        highId.Entity(key.Data).AddNeighbor(lowId.Entity(key.Data), hiChain, key);
        var b = new MapPolygonEdge(
            key.Data.IdDispenser.TakeId(), 0f, lowId, highId,
            new Dictionary<byte, byte>(), new Dictionary<byte, byte>(),
            ERef<MapPolyNexus>.GetEmpty(), ERef<MapPolyNexus>.GetEmpty());
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
        var oldSegs = this.GetSegsRel(poly, data).Segments;
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
        var hiPoly = HighPoly.Entity(key.Data);
        var loPoly = LowPoly.Entity(key.Data);
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
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }

    public void SetNexi(MapPolyNexus n1, MapPolyNexus n2, GenWriteKey key)
    {
        HiNexus = n1.MakeRef();
        LoNexus = n2.MakeRef();
    }

    public bool IsIncidentToNexus(MapPolyNexus nexus)
    {
        if (nexus == null) return false;
        return HiNexus.RefId == nexus.Id || LoNexus.RefId == nexus.Id;
    }

    public MapPolyNexus GetOtherNexus(MapPolyNexus nexus, Data data)
    {
        if (IsIncidentToNexus(nexus) == false) throw new Exception();
        return HiNexus.RefId == nexus.Id ? LoNexus.Entity(data) : HiNexus.Entity(data);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}

