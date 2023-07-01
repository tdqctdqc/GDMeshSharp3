using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public partial class MapPolygon : Entity, 
    IReadOnlyGraphNode<MapPolygon, PolyBorderChain>
{
    public override Type GetDomainType() => DomainType();
    private static Type DomainType() => typeof(PlanetDomain);
    public override EntityTypeTreeNode GetEntityTypeTreeNode() => EntityTypeTreeNode;
    public static EntityTypeTreeNode EntityTypeTreeNode { get; private set; }
    public Vector2 Center { get; protected set; }
    public EntityRefCollection<MapPolygon> Neighbors { get; protected set; }
    public Dictionary<int, PolyBorderChain> NeighborBorders { get; protected set; }
    public Color Color { get; protected set; }
    public float Altitude { get; protected set; }
    public float Roughness { get; protected set; }
    public float Moisture { get; protected set; }
    public EntityRef<Regime> Regime { get; protected set; }
    public PolyTris Tris { get; protected set; }
    public bool IsLand { get; protected set; }
    public EmploymentReport Employment { get; private set; }
    public PolyBuildingSlots PolyBuildingSlots { get; private set; }
    public PolyFoodProd PolyFoodProd { get; private set; }
    [SerializationConstructor] private MapPolygon(int id, Vector2 center, EntityRefCollection<MapPolygon> neighbors, 
        Dictionary<int, PolyBorderChain> neighborBorders, Color color, float altitude, float roughness, 
        float moisture, EntityRef<Regime> regime, PolyTris tris, bool isLand,
        EmploymentReport employment, PolyBuildingSlots polyBuildingSlots, PolyFoodProd polyFoodProd) 
            : base(id)
    {
        Center = center;
        Neighbors = neighbors;
        NeighborBorders = neighborBorders;
        Color = color;
        Altitude = altitude;
        Roughness = roughness;
        Moisture = moisture;
        Regime = regime;
        Tris = tris;
        IsLand = isLand;
        Employment = employment;
        PolyBuildingSlots = polyBuildingSlots;
        PolyFoodProd = polyFoodProd;
    }

    public static MapPolygon Create(Vector2 center, float mapWidth, GenWriteKey key)
    {
        var mapCenter = center;
        if (mapCenter.X > mapWidth) mapCenter = new Vector2(mapCenter.X - mapWidth, center.Y);
        if (mapCenter.X < 0f) mapCenter = new Vector2(mapCenter.X + mapWidth, center.Y);

        var id = key.IdDispenser.GetID();
        
        var p = new MapPolygon(id, mapCenter,
            EntityRefCollection<MapPolygon>.Construct(new HashSet<int>(), key.Data),
            new Dictionary<int, PolyBorderChain>(),
            ColorsExt.GetRandomColor(),
            0f,
            0f,
            0f,
            new EntityRef<Regime>(-1),
            null,
            true,
            EmploymentReport.Construct(),
            PolyBuildingSlots.Construct(),
            PolyFoodProd.Construct()
        );
        key.Create(p);
        return p;
    }
    
    public void AddNeighbor(MapPolygon n, PolyBorderChain border, GenWriteKey key)
    {
        if (Neighbors.Contains(n)) return;
        Neighbors.AddRef(n, key);
        NeighborBorders.Add(n.Id, border);
    }
    public void SetNeighborBorder(MapPolygon n, PolyBorderChain border, GenWriteKey key)
    {
        if (Neighbors.Contains(n) == false) throw new Exception();
        NeighborBorders[n.Id] = border;
    }
    public void RemoveNeighbor(MapPolygon poly, GenWriteKey key)
    {
        //only use in merging left-right wrap
        Neighbors.RemoveRef(poly, key);
    }
    public void SetRegime(Regime r, CreateWriteKey key)
    {
        GetMeta().UpdateEntityVar<EntityRef<Regime>>(nameof(Regime), this, key, new EntityRef<Regime>(r.Id));
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

    public void SetEmploymentReport(EmploymentReport employment, ProcedureWriteKey key)
    {
        Employment.Copy(employment, key);
    }

    public void SetAltitude(float altitude, GenWriteKey key)
    {
        Altitude = altitude;
    }
    PolyBorderChain IReadOnlyGraphNode<MapPolygon, PolyBorderChain>.GetEdge(MapPolygon neighbor) =>
        this.GetBorder(neighbor.Id);
    
    MapPolygon IReadOnlyGraphNode<MapPolygon>.Element => this;

    IReadOnlyCollection<MapPolygon> IReadOnlyGraphNode<MapPolygon>.Neighbors => Neighbors;

    bool IReadOnlyGraphNode<MapPolygon>.HasNeighbor(MapPolygon neighbor) => Neighbors.RefIds.Contains(neighbor.Id);
}
