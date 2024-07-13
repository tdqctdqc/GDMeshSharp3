using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Godot;

public class Models
{
    private Dictionary<Type, IModelManager> _managers;
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
    public FlowList Flows { get; private set; }
    public Troops Troops { get; private set; }
    public MoveTypes MoveTypes { get; private set; }
    public TroopDomains TroopDomains { get; private set; }
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
        
        
        _managers = new Dictionary<Type, IModelManager>();
        ModelsById = new Dictionary<int, IModel>();
        _idIter = 0;
        
        
        Items = new Items();
        AddManager(Items, _depot);

        Landforms = new LandformList();
        AddManager(Landforms, _depot);

        Vegetations = new VegetationList(Landforms);
        AddManager(Vegetations, _depot);
        
        PeepJobs = new PeepJobList();
        AddManager(PeepJobs, _depot);
        
        Flows = new FlowList();
        AddManager(Flows, _depot);

        Buildings = new BuildingList();
        AddManager(Buildings, _depot);

        RoadList = new RoadList();
        AddManager(RoadList, _depot);

        Settlements = new SettlementTierList();
        AddManager(Settlements, _depot);
        
        Cultures = new CultureManager();
        AddManager(Cultures, _depot);
        
        RegimeTemplates = new RegimeTemplateManager(Cultures);
        AddManager(RegimeTemplates, _depot);
        
        FoodProdTechniques = new FoodProdTechniqueList(PeepJobs, Items);
        AddManager(FoodProdTechniques, _depot);
        //
        // Infras = new InfraList(PeepJobs, Items);
        // AddManager(Infras, _depot);
        
        MoveTypes = new MoveTypes();
        AddManager(MoveTypes, _depot);
        
        Troops = new Troops();
        AddManager(Troops, _depot);

        ResourceExtractions = new ResourceExtractionList();
        AddManager(ResourceExtractions, _depot);
        
        TroopDomains = new TroopDomains();
        AddManager(TroopDomains, _depot);
        
    }

    private void SetId(IModel model)
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
        return GetManager<TModel>().Models;
    }
    public IModelManager<TModel> GetManager<TModel>() where TModel : IModel
    {
        return (IModelManager<TModel>)_managers[typeof(TModel)];
    }
    private void AddManager<T>(IModelManager<T> manager,
        DepotImporter importer) 
        where T : IModel
    {
        _managers.Add(typeof(T), manager);
        _depot.MakeSheetObjectsModels(manager);
        foreach (var (name, model) in manager.ByName)
        {
            try
            {
                SetId(model);
            }
            catch (Exception e)
            {
                GD.Print($"couldnt set id for {typeof(T).Name} {name}");
                throw;
            }
        }
    }
    
}