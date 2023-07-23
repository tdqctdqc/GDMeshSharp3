using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class AuxData<TEntity> : IEntityAux
    where TEntity : Entity
{
    public Type EntityType => typeof(TEntity);
    public abstract void HandleAdded(TEntity added);
    
    public abstract void HandleRemoved(TEntity removing);

    public AuxData(Data data)
    {
        data.SubscribeForCreation<TEntity>(n =>
        {
            HandleAdded((TEntity) n.Entity);
        });
        data.SubscribeForDestruction<TEntity>(n => HandleRemoved((TEntity)n.Entity));
    }
}