using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IEntityVarMeta
{
    object GetForSerialize(Entity e);
    bool Test(Entity t, Data data);
}
public interface IEntityVarMeta<TEntity> : IEntityVarMeta
    where TEntity : Entity
{
}