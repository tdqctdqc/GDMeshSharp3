using Godot;
using System;
using System.Collections.Generic;

public class EntityAux<T> : IEntityAux where T : Entity
{
    public EntityRegister<T> Register { get; private set; }
    Type IEntityAux.EntityType => typeof(T);
    public EntityAux(Data data)
    {
        Register = data.GetRegister<T>();
    }
}
