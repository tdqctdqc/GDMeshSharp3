using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class MapPolygon : Entity
{
    public Vector2 Center { get; protected set; }
    public EntRefCol<MapPolygon> Neighbors { get; protected set; }
    public Dictionary<int, PolyBorderChain> NeighborBorders { get; protected set; }
    public Color Color { get; protected set; }
    public float Altitude { get; protected set; }
    public float Roughness { get; protected set; }
    public float Moisture { get; protected set; }
    public EntityRef<Regime> OwnerRegime { get; protected set; }
    public EntityRef<Regime> OccupierRegime { get; private set; }
    public PolyTris Tris { get; protected set; }
    public bool IsLand { get; protected set; }
    public PolyBuildingSlots PolyBuildingSlots { get; private set; }
    public PolyFoodProd PolyFoodProd { get; private set; }
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, EntRefCol<MapPolygon> neighbors, 
        Dictionary<int, PolyBorderChain> neighborBorders, Color color, float altitude, float roughness, 
        float moisture, EntityRef<Regime> ownerRegime, 
        EntityRef<Regime> occupierRegime,
        PolyTris tris, bool isLand,
        PolyBuildingSlots polyBuildingSlots, PolyFoodProd polyFoodProd) 
            : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        NeighborBorders = neighborBorders;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        OwnerRegime = ownerRegime;
        OccupierRegime = occupierRegime;
        Tris = tris;
        IsLand = isLand;
        PolyBuildingSlots = polyBuildingSlots;
        PolyFoodProd = polyFoodProd;
    }

    public static MapPolygon Create(Vector2 center, float mapWidth, GenWriteKey key)
    {
        var mapCenter = center;
        if (mapCenter.X > mapWidth) mapCenter = new Vector2(mapCenter.X - mapWidth, center.Y);
        if (mapCenter.X < 0f) mapCenter = new Vector2(mapCenter.X + mapWidth, center.Y);

        var id = key.Data.IdDispenser.TakeId();
        var p = new MapPolygon(id, mapCenter,
            EntRefCol<MapPolygon>
                .Construct(nameof(Neighbors), 
                    id, new HashSet<int>(), key.Data),
            new Dictionary<int, PolyBorderChain>(),
            ColorsExt.GetRandomColor(),
            0f,
            0f,
            0f,
            new EntityRef<Regime>(-1),
            new EntityRef<Regime>(-1),
            null,
            true,
            PolyBuildingSlots.Construct(),
            PolyFoodProd.Construct()
        );
        key.Create(p);
        return p;
    }
    
    public void AddNeighbor(MapPolygon n, PolyBorderChain border, StrongWriteKey key)
    {
        if (Neighbors.Contains(n)) return;
        Neighbors.Add(n, key);
        NeighborBorders.Add(n.Id, border);
    }
    public void SetNeighborBorder(MapPolygon n, PolyBorderChain border, StrongWriteKey key)
    {
        if (Neighbors.Contains(n) == false) throw new Exception();
        NeighborBorders[n.Id] = border;
    }
    public void RemoveNeighbor(MapPolygon poly, StrongWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.Remove(poly, key);
    }
    public void SetOwnerRegime(Regime r, StrongWriteKey key)
    {
        var old = OwnerRegime.Entity(key.Data);
        OwnerRegime = r.MakeRef();
        key.Data.Planet.PolygonAux.ChangedOwnerRegime.Invoke(this, r, old);
    }
    public void SetOccupierRegime(Regime r, StrongWriteKey key)
    {
        var old = OccupierRegime.Entity(key.Data);
        OccupierRegime = r.MakeRef();
        key.Data.Planet.PolygonAux.ChangedOccupierRegime.Invoke(this, r, old);
    }
    public void SetTerrainTris(PolyTris tris, GenWriteKey key)
    {
        Tris = tris;
    }

    public void SetTerrainStats(GenWriteKey key)
    {
        PolyBuildingSlots.SetSlotNumbers(this, key);
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
}
