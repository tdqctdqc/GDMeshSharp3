using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public interface IRef
{
    void SyncRef(Data data);
    void ClearRef();
}

public interface IReadOnlyRefCollection : IRef
{
    int OwnerEntityId { get; }
    void UpdateOwnerId(int newId, StrongWriteKey key);
}
public interface IReadOnlyRefCollection<TRef> : IReadOnlyRefCollection
{
    IReadOnlyList<TRef> Items(Data data);
}
public interface IRefCollection<TRef> : IReadOnlyRefCollection<TRef>
{
    void Add(List<TRef> ids, StrongWriteKey key);
    void Add(TRef id, StrongWriteKey key);
    void Remove(List<TRef> ids, StrongWriteKey key);
    void Remove(TRef id, StrongWriteKey key);
}



