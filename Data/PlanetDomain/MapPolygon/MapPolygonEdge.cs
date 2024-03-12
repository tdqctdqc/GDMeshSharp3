using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using VoronoiSandbox;


public class MapPolygonEdge : Entity
{
    public float MoistureFlow { get; protected set; }
    public ERef<MapPolygon> LowPoly { get; protected set; }
    public ERef<MapPolygon> HighPoly { get; protected set; }
    public ERef<MapPolyNexus> HiNexus { get; private set; }
    public ERef<MapPolyNexus> LoNexus { get; private set; }
    [SerializationConstructor] private MapPolygonEdge(int id, float moistureFlow, 
        ERef<MapPolygon> lowPoly, ERef<MapPolygon> highPoly, 
        ERef<MapPolyNexus> loNexus, ERef<MapPolyNexus> hiNexus) 
        : base(id)
    {
        MoistureFlow = moistureFlow;
        LowPoly = lowPoly;
        HighPoly = highPoly;
        LoNexus = loNexus;
        HiNexus = hiNexus;
    }
    public static MapPolygonEdge Create(PreEdge pre, 
        GenWriteKey key)
    {
        var (hiPrePoly, loPrePoly) = pre.P1.Id > pre.P2.Id
            ? (pre.P1, pre.P2)
            : (pre.P2, pre.P1);
        var (hi, lo) = (key.Data.Get<MapPolygon>(hiPrePoly.Id),
            key.Data.Get<MapPolygon>(loPrePoly.Id));
        lo.AddNeighbor(hi, key);
        hi.AddNeighbor(lo, key);
        
        var (hiPreNexus, loPreNexus) = pre.N1.Id > pre.N2.Id 
            ? (pre.N1, pre.N2)
            : (pre.N2, pre.N1);
        var (hiNexus, loNexus) = 
            (key.Data.Get<MapPolyNexus>(hiPreNexus.Id),
                key.Data.Get<MapPolyNexus>(loPreNexus.Id));
        
        var b = new MapPolygonEdge(
            pre.Id, 0f, 
            lo.MakeRef(), hi.MakeRef(),
            loNexus.MakeRef(), 
            hiNexus.MakeRef());
        key.Create(b);
        return b;
    }
    
    
    
    public void IncrementFlow(float increment, GenWriteKey key)
    {
        MoistureFlow += increment;
    }


    public bool IsIncidentToNexus(MapPolyNexus nexus)
    {
        if (nexus == null) return false;
        return HiNexus.RefId == nexus.Id || LoNexus.RefId == nexus.Id;
    }

    public MapPolyNexus GetOtherNexus(MapPolyNexus nexus, Data data)
    {
        if (IsIncidentToNexus(nexus) == false) throw new Exception();
        return HiNexus.RefId == nexus.Id ? LoNexus.Get(data) : HiNexus.Get(data);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}

