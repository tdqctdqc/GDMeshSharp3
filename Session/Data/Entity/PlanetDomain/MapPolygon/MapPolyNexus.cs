using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class MapPolyNexus : Entity
{
    public Vector2 Point { get; private set; }
    public EntityRefCollection<MapPolygonEdge> IncidentEdges { get; private set; }
    public EntityRefCollection<MapPolygon> IncidentPolys { get; private set; }

    public static MapPolyNexus Construct(Vector2 point, List<MapPolygonEdge> edges, List<MapPolygon> polys,
        GenWriteKey key)
    {
        var n = new MapPolyNexus(key.IdDispenser.GetID(), point, 
            EntityRefCollection<MapPolygonEdge>.Construct(edges.Select(e => e.Id).ToHashSet(), key.Data),
            EntityRefCollection<MapPolygon>.Construct(polys.Select(p => p.Id).ToHashSet(), key.Data));
        key.Create(n);
        return n;
    }
    [SerializationConstructor] private MapPolyNexus(int id, Vector2 point, EntityRefCollection<MapPolygonEdge> incidentEdges,
        EntityRefCollection<MapPolygon> incidentPolys) : base(id)
    {
        Point = point;
        IncidentEdges = incidentEdges;
        IncidentPolys = incidentPolys;
    }

    public MapPolygonEdge GetEdgeWith(MapPolyNexus n, Data data)
    {
        return IncidentEdges.Entities(data).First(e => e.HiNexus.Entity(data) == n
                                    || e.LoNexus.Entity(data) == n);
    }

    public IEnumerable<MapPolyNexus> GetNeighbors(Data data)
    {
        return IncidentEdges.Entities(data).Select(e =>
        {
            if (e.HiNexus.Entity(data) == this) return e.LoNexus.Entity(data);
            return e.HiNexus.Entity(data);
        });
    }
    

    public void SetPoint(Vector2 point, GenWriteKey key)
    {
        Point = point;
    }
    //ENTITY NECESSARIES
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }

    
}
