using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class SingletonCache<T> where T : Entity
{
    private Data _data;
    public T Value => _data.GetAll<T>().FirstOrDefault();
    public SingletonCache(Data data)
    {
        _data = data;
        _data.SubscribeForCreation<T>(
            entity => { if (_data.GetAll<T>().Count() > 1) throw new Exception(); }
        );
    }
}