using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class BuildingModelManager : IModelManager<BuildingModel>
{
    public Dictionary<string, BuildingModel> Models { get; private set; }
    // public static Farm Farm { get; private set; } = new Farm();
    public static Mine IronMine { get; private set; } = new Mine(nameof(IronMine), ItemManager.Iron);
    public static Factory Factory { get; private set; } = new Factory();
    public static TownHall TownHall { get; private set; } = new TownHall();
    // public static Ranch Ranch { get; private set; } = new Ranch();
    public static Dictionary<Item, Mine> Mines { get; private set; }
        
    public BuildingModelManager()
    {
        var buildings = GetType().GetStaticPropertiesOfType<BuildingModel>();
        Models = buildings.ToDictionary(b => b.Name, b => b);
        Mines = buildings.SelectWhereOfType<BuildingModel, Mine>()
            .ToDictionary(b => b.ProdItem, b => b);
    }
    private void AddBuildings(params BuildingModel[] models)
    {
        foreach (var model in models)
        {
            Models.Add(model.Name, model);
        }
    }
}
