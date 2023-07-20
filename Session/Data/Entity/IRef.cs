using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IRef
{
    void SyncRef(Data data);
    void ClearRef();
}


public interface IRefCollection<TKey> : IRef
{
    void Add(Entity e, List<TKey> ids, StrongWriteKey key);
    void Add(Entity e, TKey id, StrongWriteKey key);
    void Remove(Entity e, List<TKey> ids, StrongWriteKey key);
    void Remove(Entity e, TKey id, StrongWriteKey key);
}



