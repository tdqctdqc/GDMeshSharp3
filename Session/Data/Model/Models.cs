using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

public class Models
{
    private Dictionary<Type, IModelManager> _managers;
    public IModel this[int id] => _models.TryGetValue(id, out var val) 
        ? (IModel) val
        : null;
    
    public Dictionary<int, IModel> _models;
    public Dictionary<string, IModel> _modelsByName;
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
    public InfraList Infras { get; private set; }
    public FlowList Flows { get; private set; }
    public Models(Data data)
    {
        _managers = new Dictionary<Type, IModelManager>();
        _models = new Dictionary<int, IModel>();
        _modelsByName = new Dictionary<string, IModel>();
        
        Items = new Items();
        AddManager(Items);

        Landforms = new LandformList();
        AddManager(Landforms);

        Vegetations = new VegetationList(Landforms);
        AddManager(Vegetations);
        
        PeepJobs = new PeepJobList();
        AddManager(PeepJobs);
        
        Flows = new FlowList();
        AddManager(Flows);

        Buildings = new BuildingList(Items, Flows, PeepJobs);
        AddManager(Buildings);

        RoadList = new RoadList();
        AddManager(RoadList);

        Settlements = new SettlementTierList();
        AddManager(Settlements);
        
        Cultures = new CultureManager();
        AddManager(Cultures);
        
        RegimeTemplates = new RegimeTemplateManager(Cultures);
        AddManager(RegimeTemplates);
        
        FoodProdTechniques = new FoodProdTechniqueList(PeepJobs);
        AddManager(FoodProdTechniques);

        Infras = new InfraList(PeepJobs, Items);
        AddManager(Infras);

        SetIds();
    }

    private void SetIds()
    {
        var ms = _modelsByName.Values.OrderBy(m => m.Name).ToList();
        for (var i = 0; i < ms.Count; i++)
        {
            var model = ms[i];
            var type = model.GetType();


            MethodInfo setter = null;
            while (setter == null)
            {
                var idProp = type.GetProperty(nameof(IModel.Id));
                setter = idProp.GetSetMethod(true);

                if (setter != null)
                {
                    setter.Invoke(model, new object[] {i});
                    _models.Add(model.Id, model);
                }
                else
                {
                    type = type.BaseType;
                    if(type == null) throw new Exception();
                }
            }
        }
    }
    public T GetModel<T>(int id)
    {
        return (T)_models[id];
    }

    public Dictionary<string, TModel> GetModels<TModel>() where TModel : IModel
    {
        return GetManager<TModel>().Models;
    }
    public IModelManager<TModel> GetManager<TModel>() where TModel : IModel
    {
        return (IModelManager<TModel>)_managers[typeof(TModel)];
    }
    private void AddManager<T>(IModelManager<T> manager) where T : IModel
    {
        _managers.Add(typeof(T), manager);
        foreach (var keyValuePair in manager.Models)
        {
            _modelsByName.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
    private void AddManager<T>(ModelList<T> list) where T : IModel
    {
        var manager = new ModelManager<T>(list);
        _managers.Add(typeof(T), manager);
        foreach (var keyValuePair in manager.Models)
        {
            _modelsByName.Add(keyValuePair.Key, keyValuePair.Value);
        }
    }
}