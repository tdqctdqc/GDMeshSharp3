using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using VoronoiSandbox;

public class MapPolygon : Entity
{
    public Vector2 Center { get; protected set; }
    public Vector2 GraphicalCenter() => BoundaryPoints.Avg() + Center; 
    public Vector2[] BoundaryPoints { get; private set; }
    public ERefSet<MapPolygon> Neighbors { get; protected set; }
    public float Altitude { get; protected set; }
    public float Roughness { get; protected set; }
    public float Moisture { get; protected set; }
    public ERef<Regime> OwnerRegime { get; protected set; }
    public ERef<Regime> OccupierRegime { get; private set; }
    public bool IsLand { get; protected set; }
    public FoodProd FoodProd { get; private set; }
    [SerializationConstructor] private MapPolygon(int id, 
        Vector2 center, ERefSet<MapPolygon> neighbors, 
        float altitude, float roughness, 
        float moisture, ERef<Regime> ownerRegime, 
        ERef<Regime> occupierRegime,
        bool isLand,
        FoodProd foodProd,
        Vector2[] boundaryPoints) 
            : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        OwnerRegime = ownerRegime;
        OccupierRegime = occupierRegime;
        IsLand = isLand;
        FoodProd = foodProd;
        BoundaryPoints = boundaryPoints;
    }

    public static MapPolygon Create(PrePoly pre, 
        int mapWidth, GenWriteKey key)
    {
        var mapCenter = pre.RelTo;
        if (mapCenter.X > mapWidth) mapCenter = new Vector2I(mapCenter.X - mapWidth, mapCenter.Y);
        if (mapCenter.X < 0f) mapCenter = new Vector2I(mapCenter.X + mapWidth, mapCenter.Y);

        var id = pre.Id;

        var preCells = pre.Cells;

        var boundaryPoints = ConstructBoundaryPoints(
            id, mapCenter, preCells, key.Data);
        
        var p = new MapPolygon(id, mapCenter,
            ERefSet<MapPolygon>
                .Construct(nameof(Neighbors), 
                    id, new HashSet<int>(), key.Data),
            0f,
            0f,
            0f,
            new ERef<Regime>(-1),
            new ERef<Regime>(-1),
            true,
            FoodProd.Construct(),
            boundaryPoints
        );
        key.Create(p);
        return p;
    }

    private static Vector2[] ConstructBoundaryPoints(
        int id,
        Vector2 center,
        List<PreCell> preCells, Data d)
    {
        var edges = new List<(Vector2, Vector2)>();
        var nIds = new List<int>();
        for (var i = 0; i < preCells.Count; i++)
        {
            var c = preCells[i];
            for (var j = 0; j < c.Neighbors.Count; j++)
            {
                var n = c.Neighbors[j];
                if (n.PrePoly.Id == id) continue;
                var edge = c.EdgesRel[j];
                edges.Add((
                    center.Offset(edge.Item1 + c.RelTo, d),
                    center.Offset(edge.Item2 + c.RelTo, d)
                ));
                nIds.Add(n.Id);
            }
        }

        return edges.Select(e => new LineSegment(e.Item1, e.Item2))
            .ToList().FlipChainify().GetPoints().ToArray();
    }
    public void AddNeighbor(MapPolygon n, 
        GenWriteKey key)
    {
        if (Neighbors.Contains(n)) return;
        Neighbors.Add(n, key);
    }
    public void RemoveNeighbor(MapPolygon poly, GenWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.Remove(poly, key);
    }

    public void SetInitialRegime(Regime r, GenWriteKey key)
    {
        SetOwnerRegime(r, key);
        SetOccupierRegime(r, key);
    }
    public void SetOwnerRegime(Regime r, StrongWriteKey key)
    {
        var old = OwnerRegime.Get(key.Data);
        OwnerRegime = r.MakeRef();
        key.Data.Notices.Political.ChangedOwnerRegime.Invoke(this, r, old);
    }
    public void SetOccupierRegime(Regime r, StrongWriteKey key)
    {
        var old = OccupierRegime.Get(key.Data);
        OccupierRegime = r.MakeRef();
        key.Data.Notices.Political.ChangedOccupierRegime.Invoke(this, r, old);
    }

    public void SetIsLand(bool isLand, GenWriteKey key)
    {
        IsLand = isLand;
    }

    

    public void SetAltitude(float altitude, GenWriteKey key)
    {
        Altitude = altitude;
    }
    public void SetRoughness(float roughness, GenWriteKey key)
    {
        Roughness = roughness;
    }

    public void SetMoisture(float moisture, GenWriteKey key)
    {
        Moisture = moisture;
    }

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
