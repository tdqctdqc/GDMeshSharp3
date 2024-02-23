using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

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
    public PolyBuildingSlots PolyBuildingSlots { get; private set; }
    public PolyFoodProd PolyFoodProd { get; private set; }
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, ERefSet<MapPolygon> neighbors, 
        float altitude, float roughness, 
        float moisture, ERef<Regime> ownerRegime, 
        ERef<Regime> occupierRegime,
        bool isLand,
        PolyBuildingSlots polyBuildingSlots, PolyFoodProd polyFoodProd) 
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
        PolyBuildingSlots = polyBuildingSlots;
        PolyFoodProd = polyFoodProd;
    }

    public static MapPolygon Create(PrePoly pre, 
        float mapWidth, GenWriteKey key)
    {
        var mapCenter = pre.RelTo;
        if (mapCenter.X > mapWidth) mapCenter = new Vector2(mapCenter.X - mapWidth, mapCenter.Y);
        if (mapCenter.X < 0f) mapCenter = new Vector2(mapCenter.X + mapWidth, mapCenter.Y);

        var id = pre.Id;
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
            PolyBuildingSlots.Construct(),
            PolyFoodProd.Construct()
        );
        key.Create(p);
        return p;
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

    public override void CleanUp(StrongWriteKey key)
    {
        
    }
}
