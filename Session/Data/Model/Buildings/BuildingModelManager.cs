using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class BuildingModelManager : IModelManager<BuildingModel>
{
    public Dictionary<string, BuildingModel> Models { get; private set; }
    // public static Farm Farm { get; private set; } = new Farm();
    public static Mine IronMine { get; private set; } = new Mine(nameof(IronMine), ItemManager.Iron);
    public static Mine CoalMine { get; private set; } = new Mine(nameof(CoalMine), ItemManager.Coal);
    public static Factory Factory { get; private set; } = new Factory();
    public static TownHall TownHall { get; private set; } = new TownHall();
    // public static Ranch Ranch { get; private set; } = new Ranch();
        
    public BuildingModelManager()
    {
        var buildings = GetType().GetStaticPropertiesOfType<BuildingModel>();
        Models = buildings.ToDictionary(b => b.Name, b => b);
    }
}
