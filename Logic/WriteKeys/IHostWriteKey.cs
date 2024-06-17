using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IHostWriteKey 
{
    Data Data { get; }
    void Create<TEntity>(TEntity t) where TEntity : Entity;
    void SendMessage(Message m);
    
}