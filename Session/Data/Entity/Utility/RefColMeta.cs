using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RefColMeta<TEntity, TColMember> : IRefColMeta<TColMember> where TEntity : Entity
{
    public RefAction<(TEntity, TColMember)> Added { get; private set; }
    public RefAction<(TEntity, TColMember)> Removed { get; private set; }

    public RefColMeta()
    {
        Added = new RefAction<(TEntity, TColMember)>();
        Removed = new RefAction<(TEntity, TColMember)>();
    }

    public void RaiseAdded(Entity e, TColMember added)
    {
        Added.Invoke(((TEntity)e, added));
    }
    public void RaiseRemoved(Entity e, TColMember removed)
    {
        Removed.Invoke(((TEntity)e, removed));
    }
}

public interface IRefColMeta<TColMember> : IRefColMeta
{
    void RaiseAdded(Entity e, TColMember added);
    void RaiseRemoved(Entity e, TColMember removed);
}
public interface IRefColMeta
{
    
}
