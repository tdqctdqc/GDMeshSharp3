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
    public TechnologyCategories TechnologyCategories { get; private set; }
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
        ImportDisallowDefault(Items, _depot);

        Landforms = new LandformList();
        ImportAllowDefault(Landforms, _depot);

        Vegetations = new VegetationList(Landforms);
        ImportAllowDefault(Vegetations, _depot);
        
        PeepJobs = new PeepJobList();
        ImportAllowDefault(PeepJobs, _depot);

        Buildings = new BuildingList();
        ImportAllowDefault(Buildings, _depot);

        RoadList = new RoadList();
        ImportDisallowDefault(RoadList, _depot);

        Settlements = new SettlementTierList();
        ImportAllowDefault(Settlements, _depot);
        
        Cultures = new CultureManager();
        foreach (var culture in Cultures.Cultures)
        {
            AddModel(culture);
        }
        
        RegimeTemplates = new RegimeTemplateManager(Cultures);
        foreach (var regimeTemplate in RegimeTemplates.RegimeTemplates)
        {
            AddModel(regimeTemplate);
        }
        
        FoodProdTechniques = new FoodProdTechniqueList(PeepJobs, Items);
        ImportDisallowDefault(FoodProdTechniques, _depot);
        
        MoveTypes = new MoveTypes();
        ImportDisallowDefault(MoveTypes, _depot);
        
        Troops = new Troops();
        ImportAllowDefault(Troops, _depot);

        ResourceExtractions = new ResourceExtractionList();
        ImportDisallowDefault(ResourceExtractions, _depot);
        
        TroopDomains = new TroopDomains();
        ImportAllowDefault(TroopDomains, _depot);

        Technologies = new TechnologyList();
        ImportAllowDefault(Technologies, _depot);

        TechnologyCategories = new TechnologyCategories();
        ImportAllowDefault(TechnologyCategories, _depot);
        
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

    private void ImportAllowDefault<T>(ModelManager<T> manager,
        DepotImporter importer)
            where T : IModel, new()
    {
        AddManager(manager, () => new(), importer);
    }
    private void ImportDisallowDefault<T>(ModelManager<T> manager,
        DepotImporter importer)
        where T : IModel
    {
        AddManager(manager, () => throw new Exception(), importer);
    }
    
    private void AddManager<T>(ModelManager<T> manager,
        Func<T> defaultConstructor,
        DepotImporter importer) 
        where T : IModel
    {
        var ms = manager.GetPropertiesOfTypeByName<T>()
            .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        var models = _depot.MakeSheetObjectsModels<T>(ms, defaultConstructor);
        foreach (var model in models)
        {
            AddModel(model);
        }
    }
    
}