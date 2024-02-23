using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;


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
        var (hiPre, loPre) = pre.P1.Id > pre.P2.Id
            ? (pre.P1, pre.P2)
            : (pre.P2, pre.P1);
        var (hi, lo) = (key.Data.Get<MapPolygon>(hiPre.Id),
            key.Data.Get<MapPolygon>(loPre.Id));
        lo.AddNeighbor(hi, key);
        hi.AddNeighbor(lo, key);
        var b = new MapPolygonEdge(
            pre.Id, 0f, 
            lo.MakeRef(), hi.MakeRef(),
            ERef<MapPolyNexus>.GetEmpty(), 
            ERef<MapPolyNexus>.GetEmpty());
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
        return HiNexus.RefId == nexus.Id ? LoNexus.Entity(data) : HiNexus.Entity(data);
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }

    public void SetNexi(MapPolyNexus nexus, MapPolyNexus otherNexus, GenWriteKey key)
    {
        var (hi, lo) = nexus.Id > otherNexus.Id
            ? (nexus, otherNexus)
            : (otherNexus, nexus);
        HiNexus = hi.MakeRef();
        LoNexus = lo.MakeRef();
    }
}

