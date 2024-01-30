using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public interface IReadOnlyRefCollection
{
    int OwnerEntityId { get; }
    void UpdateOwnerId(int newId, StrongWriteKey key);
}
public interface IReadOnlyRefCollection<TRef> : IReadOnlyRefCollection
{
    IEnumerable<TRef> Items(Data data);
}
public interface IRefCollection<TRef> : IReadOnlyRefCollection<TRef>
{
    void Add(List<TRef> ids, StrongWriteKey key);
    void Add(TRef id, StrongWriteKey key);
    void Remove(List<TRef> ids, StrongWriteKey key);
    void Remove(TRef id, StrongWriteKey key);
}



