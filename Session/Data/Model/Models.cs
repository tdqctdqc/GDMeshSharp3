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
    public LandformManager Landforms { get; private set; }
    public VegetationManager Vegetation { get; private set; }
    public PeepJobManager PeepJobs { get; private set; }
    public ItemManager Items { get; private set; }
    public SettlementTierManager SettlementTiers { get; private set; }
    public BuildingModelManager Buildings { get; private set; }
    public RoadModelManager Roads { get; private set; }
    public CultureManager Cultures { get; private set; }
    public RegimeTemplateManager RegimeTemplates { get; private set; }
    public FoodProdTechniqueManager FoodProdTechniques { get; private set; }
    public Models()
    {
        _managers = new Dictionary<Type, IModelManager>();
        _models = new Dictionary<int, IModel>();
        _modelsByName = new Dictionary<string, IModel>();
        Landforms = new LandformManager();
        AddManager(Landforms);
        Vegetation = new VegetationManager();
        AddManager(Vegetation);
        PeepJobs = new PeepJobManager();
        AddManager(PeepJobs);
        Items = new ItemManager();
        AddManager(Items);
        Buildings = new BuildingModelManager();
        AddManager(Buildings);
        Roads = new RoadModelManager();
        AddManager(Roads);
        SettlementTiers = new SettlementTierManager();
        AddManager(SettlementTiers);
        Cultures = new CultureManager();
        AddManager(Cultures);
        RegimeTemplates = new RegimeTemplateManager(Cultures);
        AddManager(RegimeTemplates);
        FoodProdTechniques = new FoodProdTechniqueManager();
        AddManager(FoodProdTechniques);

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
}