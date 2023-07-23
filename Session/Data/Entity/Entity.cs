using Godot;
using System;
using System.Collections.Generic;
using MessagePack;

public abstract class Entity
{
    public int Id { get; protected set; }
    protected Entity(int id)
    {
        Id = id;
    }

    public void SetId(int id, StrongWriteKey key)
    {
        Id = id;
    }
}
