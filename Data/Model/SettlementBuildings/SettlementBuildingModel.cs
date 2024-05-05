
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class SettlementBuildingModel : IModel, IMakeable, IIconed
{
    public int Id { get; private set; }
    public string Name { get; }
    public List<BuildingModelComponent> Components { get; private set; }
    public MakeableAttribute Makeable { get; private set; }
    

    public Icon Icon { get; }
    public SettlementBuildingModel( 
        string name, 
        List<BuildingModelComponent> components, 
        MakeableAttribute makeable)
    {
        Name = name;
        Icon = Icon.Create(Name, Vector2I.One);
        Components = components;
        Makeable = makeable;
    }
    
    public abstract bool CanBuildInCell(Cell t, Data data);
    public abstract bool CanBuildInPoly(MapPolygon p, Data data);
    
    public T GetComponent<T>(Func<T, bool> good) where T : BuildingModelComponent
    {
        return (T) Components.FirstOrDefault(c => c is T t && good(t));
    }
    public T GetComponent<T>() where T : BuildingModelComponent
    {
        return (T) Components.FirstOrDefault(c => c is T t);
    }
    
    public bool HasComponent<T>() where T : BuildingModelComponent
    {
        return Components.Any(c => c is T t);
    }
}
