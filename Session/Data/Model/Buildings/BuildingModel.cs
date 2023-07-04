
using System.Collections.Generic;
using Godot;

public abstract class BuildingModel : IModel
{
    public int Id { get; private set; }
    public string Name { get; }
    public int NumTicksToBuild { get; private set; }
    public int ConstructionCapPerTick { get; private set; }
    public BuildingType BuildingType { get; private set; }
    public Icon Icon { get; }
    public abstract Dictionary<Item, int> BuildCosts { get; protected set; }

    public BuildingModel(BuildingType buildingType, string name, int numTicksToBuild, int constructionCapPerTick)
    {
        BuildingType = buildingType;
        Name = name;
        NumTicksToBuild = numTicksToBuild;
        ConstructionCapPerTick = constructionCapPerTick;
        Icon = Icon.Create(Name, Icon.AspectRatio._1x1, 25f);
    }

    protected abstract bool CanBuildInTriSpec(PolyTri t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);
    public bool CanBuildInTri(PolyTri t, Data data)
    {
        return t.GetBuilding(data) == null && CanBuildInTriSpec(t, data);
    }
}
