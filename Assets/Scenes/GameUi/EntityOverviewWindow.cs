using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class EntityOverviewWindow : Window
{
    public static EntityOverviewWindow Get(Data data)
    {
        var eo = SceneManager.Instance<EntityOverviewWindow>();
        eo.Setup(data);
        eo.Hide();
        return eo;
    }
    private UIVar<Domain> _domain;
    private UIVar<Type> _entityType;
    private UIVar<Entity> _selectedEntity;

    private ItemListToken _domainToken, _entityTypeToken, _entityTypePropsToken, 
        _entitiesToken, _entityPropsToken;
    private Data _data;

    private void Setup(Data data)
    {
        _data = data;
        
        _domain = new UIVar<Domain>(null);
        _domain.ChangedValue += DrawEntityTypes;
        
        _entityType = new UIVar<Type>(null);
        _entityType.ChangedValue += DrawEntitiesOfType;
        
        _selectedEntity = new UIVar<Entity>(null);
        _selectedEntity.ChangedValue += DrawEntityProps;
        
        _domainToken = ItemListToken.Construct((ItemList) FindChild("Domains"));
        _entityTypeToken = ItemListToken.Construct((ItemList) FindChild("EntityTypes"));
        _entitiesToken = ItemListToken.Construct((ItemList) FindChild("Entities"));
        _entityPropsToken = ItemListToken.Construct((ItemList) FindChild("EntityProps"));
        _entityTypePropsToken = ItemListToken.Construct((ItemList) FindChild("EntityTypeProps"));
        AboutToPopup += Draw;
    }
    private void Draw()
    {
        _domainToken.Setup(_data.Domains.Values.ToList(), 
            d => d.GetType().Name,
            d => () => _domain.SetValue(d));
    }

    private void DrawEntityTypes(Domain d)
    {
        _entityTypeToken.Setup(d.Registers.Select(r => r.Value.EntityType).ToList(),
            t => t.Name,
            t => () => _entityType.SetValue(t));
    }

    private void DrawEntitiesOfType(Type type)
    {
        var d = _domain.Value;
        var register = d.Registers[type];
        
        _entitiesToken.Setup(register.Entities.ToList(), 
            e => e.Id.ToString(),
            e => () => _selectedEntity.SetValue(e));
        
        _entityTypePropsToken.Setup<string>(
            new List<string>
            {
                "Number: " + register.Entities.Count
            },
            i => i,
            i => () => { }
        );
    }
    private void DrawEntityProps(Entity e)
    {
        var meta = e.GetMeta();
        var vals = meta.GetPropertyValues(e);
        _entityPropsToken.Setup<int>(
            Enumerable.Range(0, e.GetMeta().FieldNameList.Count).ToList(),
            i => meta.FieldNameList[i] + ": " + vals[i].ToString(),
            i => () => { }
        );
    }
}
