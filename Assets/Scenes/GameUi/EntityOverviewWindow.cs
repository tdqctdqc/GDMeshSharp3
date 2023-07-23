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
        
        
        _entityType = new UIVar<Type>(null);
        _entityType.ChangedValue += DrawEntitiesOfType;
        
        _selectedEntity = new UIVar<Entity>(null);
        _selectedEntity.ChangedValue += e => DrawEntityProps(data, e);
        
        _entityTypeToken = ItemListToken.Construct((ItemList) FindChild("EntityTypes"));
        _entitiesToken = ItemListToken.Construct((ItemList) FindChild("Entities"));
        _entityPropsToken = ItemListToken.Construct((ItemList) FindChild("EntityProps"));
        _entityTypePropsToken = ItemListToken.Construct((ItemList) FindChild("EntityTypeProps"));
        AboutToPopup += Draw;
    }
    private void Draw()
    {
        DrawEntityTypes();

    }

    private void DrawEntityTypes()
    {
        _entityTypeToken.Setup(_data.Registers.Select(r => r.Value.EntityType).ToList(),
            t => t.Name,
            t => () => _entityType.SetValue(t));
    }

    private void DrawEntitiesOfType(Type type)
    {
        var entities = _data.Entities.Values.Where(e => type.IsAssignableFrom(e.GetType()) ).ToList();
        
        _entitiesToken.Setup(entities, 
            e => e.Id.ToString(),
            e => () => _selectedEntity.SetValue(e));
        
        _entityTypePropsToken.Setup<string>(
            new List<string>
            {
                "Number: " + entities.Count
            },
            i => i,
            i => () => { }
        );
    }
    private void DrawEntityProps(Data data, Entity e)
    {
        var meta = data.Serializer.GetEntityMeta(e.GetType());
        var vals = meta.GetPropertyValues(e);
        _entityPropsToken.Setup<int>(
            Enumerable.Range(0, meta.FieldNameList.Count).ToList(),
            i => meta.FieldNameList[i] + ": " + vals[i].ToString(),
            i => () => { }
        );
    }
}
