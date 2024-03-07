using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface ICreateWriteKey 
{
    Data Data { get; }
    void Create<TEntity>(TEntity t) where TEntity : Entity;
}