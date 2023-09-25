
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class BuildingModel : IModel
{
    public int Id { get; private set; }
    public string Name { get; }
    public int NumTicksToBuild { get; private set; }
    public int ConstructionCapPerTick { get; private set; }
    public BuildingType BuildingType { get; private set; }
    public List<BuildingModelComponent> Components { get; private set; }
    public Icon Icon { get; }
    IReadOnlyList<IModelAttribute> IModel.AttributeList => Attributes;
    public AttributeHolder<IModelAttribute> Attributes { get; private set; }
    public BuildingModel(BuildingType buildingType, string name, int numTicksToBuild, int constructionCapPerTick,
        List<BuildingModelComponent> components,
        AttributeHolder<IModelAttribute> attributes)
    {
        BuildingType = buildingType;
        Name = name;
        NumTicksToBuild = numTicksToBuild;
        ConstructionCapPerTick = constructionCapPerTick;
        Icon = Icon.Create(Name, Icon.AspectRatio._1x1, 25f);
        Components = components;
        Attributes = attributes;
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
