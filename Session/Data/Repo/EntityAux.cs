using Godot;
using System;
using System.Collections.Generic;

public class EntityAux<T> : IEntityAux where T : Entity
{
    Type IEntityAux.EntityType => typeof(T);
    public EntityAux(Data data)
    {
    }
}
