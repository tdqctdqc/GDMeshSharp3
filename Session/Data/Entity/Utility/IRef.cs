using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IRef
{
    void SyncRef(Data data);
    void ClearRef();
}

public interface IRefCollection : IRef
{
    void UpdateOwnerId(int newId, StrongWriteKey key);
}
public interface IRefCollection<TKey> : IRefCollection
{
    void Add(List<TKey> ids, StrongWriteKey key);
    void Add(TKey id, StrongWriteKey key);
    void Remove(List<TKey> ids, StrongWriteKey key);
    void Remove(TKey id, StrongWriteKey key);
}



