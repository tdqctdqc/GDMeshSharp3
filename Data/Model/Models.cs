using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;

public class Models
{
    public IModel this[int id] => ModelsById.TryGetValue(id, out var val) 
        ? (IModel) val
        : null;
    
    public Dictionary<int, IModel> ModelsById { get; private set; }
    public RoadList RoadList { get; private set; }
    public LandformList Landforms { get; private set; }
    public VegetationList Vegetations { get; private set; }
    public PeepJobList PeepJobs { get; private set; }
    public Items Items { get; private set; }
    public SettlementTierList Settlements { get; private set; }
    public BuildingList Buildings { get; private set; }
    public CultureManager Cultures { get; private set; }
    public RegimeTemplateManager RegimeTemplates { get; private set; }
    public FoodProdTechniqueList FoodProdTechniques { get; private set; }
    // public InfraList Infras { get; private set; }
    public Troops Troops { get; private set; }
    public MoveTypes MoveTypes { get; private set; }
    public TroopDomains TroopDomains { get; private set; }
    public TechnologyList Technologies { get; private set; }
    public ResourceExtractionList ResourceExtractions { get; private set; }
    private int _idIter;
    private DepotImporter _depot;
    public Models(Data data)
    {
        string filePath = Directory.GetCurrentDirectory();
        filePath += "\\depot.dpo";
        _depot = new DepotImporter(filePath);
        _depot.MakeSheetObjectsDefault<MakeableAttribute>(
            () => new MakeableAttribute(null, null));
        _depot.MakeSheetObjectsDefault<LaborComponent>(
            () => new LaborComponent(null, null, null));
        
        ModelsById = new Dictionary<int, IModel>();
        _idIter = 0;
        
        Items = new Items();
        AddManagerDisallowDefault(Items, _depot);

        Landforms = new LandformList();
        AddManagerAllowDefault(Landforms, _depot);

        Vegetations = new VegetationList(Landforms);
        AddManagerAllowDefault(Vegetations, _depot);
        
        PeepJobs = new PeepJobList();
        AddManagerAllowDefault(PeepJobs, _depot);

        Buildings = new BuildingList();
        AddManagerAllowDefault(Buildings, _depot);

        RoadList = new RoadList();
        AddManagerDisallowDefault(RoadList, _depot);

        Settlements = new SettlementTierList();
        AddManagerAllowDefault(Settlements, _depot);
        
        Cultures = new CultureManager();
        AddManagerDisallowDefault(Cultures, _depot);
        
        RegimeTemplates = new RegimeTemplateManager(Cultures);
        AddManagerDisallowDefault(RegimeTemplates, _depot);
        
        FoodProdTechniques = new FoodProdTechniqueList(PeepJobs, Items);
        AddManagerDisallowDefault(FoodProdTechniques, _depot);
        
        MoveTypes = new MoveTypes();
        AddManagerDisallowDefault(MoveTypes, _depot);
        
        Troops = new Troops();
        AddManagerAllowDefault(Troops, _depot);

        ResourceExtractions = new ResourceExtractionList();
        AddManagerDisallowDefault(ResourceExtractions, _depot);
        
        TroopDomains = new TroopDomains();
        AddManagerAllowDefault(TroopDomains, _depot);

        Technologies = new TechnologyList();
        AddManagerAllowDefault(Technologies, _depot);
        
        _depot.FillAllProperties();
        
        foreach (var m in ModelsById.Values.OfType<IIconed>())
        {
            m.CreateIcon();
        }
    }

    private void AddModel(IModel model)
    {
        var type = model.GetType();

        MethodInfo setter = null;
        while (setter == null)
        {
            var idProp = type.GetProperty(nameof(IModel.Id));
            setter = idProp.GetSetMethod(true);

            if (setter != null)
            {
                setter.Invoke(model, new object[] {_idIter});
                _idIter++;
                ModelsById.Add(model.Id, model);
            }
            else
            {
                type = type.BaseType;
                if(type == null) throw new Exception();
            }
        }
    }
    public T GetModel<T>(int id) where T : IModel
    {
        return (T)ModelsById[id];
    }

    public List<TModel> GetModels<TModel>() where TModel : IModel
    {
        return ModelsById.Values.OfType<TModel>().ToList();
    }

    private void AddManagerAllowDefault<T>(IModelManager<T> manager,
        DepotImporter importer)
            where T : IModel, new()
    {
        AddManager(manager, () => new(), importer);
    }
    private void AddManagerDisallowDefault<T>(IModelManager<T> manager,
        DepotImporter importer)
        where T : IModel
    {
        AddManager(manager, () => throw new Exception(), importer);
    }
    
    private void AddManager<T>(IModelManager<T> manager,
        Func<T> defaultConstructor,
        DepotImporter importer) 
        where T : IModel
    {
        var ms = manager.ByName
            .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        var models = _depot.MakeSheetObjectsModels<T>(ms, defaultConstructor);
        if (models == null)
        {
            models = manager.ByName.Values;
        };
        foreach (var model in models)
        {
            AddModel(model);
        }
    }
    
}