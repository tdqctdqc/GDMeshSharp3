using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public abstract class Entity : IIdentifiable
{
    public int Id { get; protected set; }
    [SerializationConstructor] protected Entity(int id)
    {
        Id = id;
    }
}
