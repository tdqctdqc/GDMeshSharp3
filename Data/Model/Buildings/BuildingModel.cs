
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class BuildingModel : IModel, IMakeable
{
    public int Id { get; private set; }
    public string Name { get; }
    public int NumTicksToBuild { get; private set; }
    public int ConstructionCapPerTick { get; private set; }
    public BuildingType BuildingType { get; private set; }
    public List<BuildingModelComponent> Components { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    public Icon Icon { get; }
    public BuildingModel(BuildingType buildingType, string name, int numTicksToBuild, int constructionCapPerTick,
        List<BuildingModelComponent> components, MakeableAttribute makeable)
    {
        BuildingType = buildingType;
        Name = name;
        NumTicksToBuild = numTicksToBuild;
        ConstructionCapPerTick = constructionCapPerTick;
        Icon = Icon.Create(Name, Vector2I.One);
        Components = components;
        Makeable = makeable;
    }

    protected abstract bool CanBuildInTriSpec(PolyTri t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);
    public bool CanBuildInTri(PolyTri t, Data data)
    {
        return t.GetBuilding(data) == null && CanBuildInTriSpec(t, data);
    }

    public T GetComponent<T>(Func<T, bool> good) where T : BuildingModelComponent
    {
        return (T) Components.FirstOrDefault(c => c is T t && good(t));
    }
    public T GetComponent<T>() where T : BuildingModelComponent
    {
        return (T) Components.FirstOrDefault(c => c is T t);
    }
    
    public bool HasComponent<T>(Func<T, bool> good) where T : BuildingModelComponent
    {
        return Components.Any(c => c is T t && good(t));
    }
    public bool HasComponent<T>() where T : BuildingModelComponent
    {
        return Components.Any(c => c is T t);
    }
}
