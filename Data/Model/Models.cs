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
    public CulturePredefs Cultures { get; private set; }
    public RegimeTemplatePredefs RegimeTemplates { get; private set; }
    public FoodProdTechniqueList FoodProdTechniques { get; private set; }
    // public InfraList Infras { get; private set; }
    public Troops Troops { get; private set; }
    public MoveTypes MoveTypes { get; private set; }
    public TroopDomains TroopDomains { get; private set; }
    public TechnologyList Technologies { get; private set; }
    public ResourceExtractionList ResourceExtractions { get; private set; }
    public TechnologyCategories TechnologyCategories { get; private set; }
    public TroopTypes TroopTypes { get; private set; }
    
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
        ImportWithPredefsDisallowDefault(Items, _depot);

        Landforms = new LandformList();
        ImportWithPredefsAllowDefault(Landforms, _depot);

        Vegetations = new VegetationList(Landforms);
        ImportWithPredefsAllowDefault(Vegetations, _depot);
        
        PeepJobs = new PeepJobList();
        ImportWithPredefsAllowDefault(PeepJobs, _depot);

        Buildings = new BuildingList();
        ImportWithPredefsAllowDefault(Buildings, _depot);

        RoadList = new RoadList();
        ImportWithPredefsDisallowDefault(RoadList, _depot);

        Settlements = new SettlementTierList();
        ImportWithPredefsAllowDefault(Settlements, _depot);
        
        Cultures = new CulturePredefs();
        foreach (var culture in Cultures.Cultures)
        {
            AddModel(culture);
        }
        
        RegimeTemplates = new RegimeTemplatePredefs(Cultures);
        foreach (var regimeTemplate in RegimeTemplates.RegimeTemplates)
        {
            AddModel(regimeTemplate);
        }
        
        FoodProdTechniques = new FoodProdTechniqueList(PeepJobs, Items);
        ImportWithPredefsDisallowDefault(FoodProdTechniques, _depot);
        
        MoveTypes = new MoveTypes();
        ImportWithPredefsDisallowDefault(MoveTypes, _depot);
        
        Troops = new Troops();
        ImportWithPredefsAllowDefault(Troops, _depot);

        ResourceExtractions = new ResourceExtractionList();
        ImportWithPredefsDisallowDefault(ResourceExtractions, _depot);
        
        TroopDomains = new TroopDomains();
        ImportWithPredefsAllowDefault(TroopDomains, _depot);

        Technologies = new TechnologyList();
        ImportWithPredefsAllowDefault(Technologies, _depot);

        TechnologyCategories = new TechnologyCategories();
        ImportWithPredefsAllowDefault(TechnologyCategories, _depot);

        TroopTypes = new TroopTypes();
        ImportWithPredefsAllowDefault<TroopType>(TroopTypes, _depot);
        
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

    private void ImportWithPredefsAllowDefault<T>(ModelPredefs<T> predefs,
        DepotImporter importer)
            where T : IModel, new()
    {
        ImportWithPredefs(predefs, () => new(), importer);
    }
    private void ImportWithPredefsDisallowDefault<T>(ModelPredefs<T> predefs,
        DepotImporter importer)
        where T : IModel
    {
        ImportWithPredefs(predefs, () => throw new Exception(), importer);
    }
    
    private void ImportWithPredefs<T>(ModelPredefs<T> predefs,
        Func<T> defaultConstructor,
        DepotImporter importer) 
        where T : IModel
    {
        var ms = predefs.GetPropertiesOfTypeByName<T>()
            .ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value);
        var models = _depot.MakeSheetObjectsModels<T>(ms, defaultConstructor);
        foreach (var model in models)
        {
            AddModel(model);
        }
    }

    private void ImportNoPredefs<T>(DepotImporter importer)
        where T : IModel, new()
    {
        var models = _depot
            .MakeSheetObjectsModels<T>(new Dictionary<string, object>(), 
                () => new T());
        foreach (var model in models)
        {
            AddModel(model);
        }
    }
    
}