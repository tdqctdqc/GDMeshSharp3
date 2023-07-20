using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public abstract class Entity
{
    public int Id { get; protected set; }
    public IEntityMeta GetMeta() => GetEntityTypeTreeNode().Meta;
    protected Entity(int id)
    {
        Id = id;  
    }

    public void Set<TValue>(string fieldName, TValue newValue, StrongWriteKey key)
    {
        GetEntityTypeTreeNode().Meta.Vars[fieldName].UpdateVar(fieldName, this, key, newValue);
    }

    public static void SubscribeToValChange<TEntity, TVal>(string valName, ValChangeAction<TVal> handler)
        where TEntity : Entity
    {
        Game.I.Serializer.GetEntityMeta<TEntity>().GetEntityVarMeta<TVal>(valName).ValChanged().Subscribe(handler);
    }
    public static void SubscribeToColAdd<TEntity, TVal>(string colName, RefAction<(TEntity, TVal)> handler)
        where TEntity : Entity
    {
        Game.I.Serializer.GetEntityMeta<TEntity>().GetRefColMeta<TVal>(colName).Added.Subscribe(handler);
    }
    public static void SubscribeToColRemove<TEntity, TVal>(string colName, RefAction<(TEntity, TVal)> handler)
        where TEntity : Entity
    {
        Game.I.Serializer.GetEntityMeta<TEntity>()
            .GetRefColMeta<TVal>(colName).Removed
            .Subscribe(handler);
    }
    public abstract Type GetDomainType();
    public abstract EntityTypeTreeNode GetEntityTypeTreeNode();
}
