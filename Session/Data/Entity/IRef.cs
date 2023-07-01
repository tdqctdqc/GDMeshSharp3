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
    void AddByProcedure(List<TKey> ids, ProcedureWriteKey key);
    void AddByProcedure(TKey id, ProcedureWriteKey key);
    void RemoveByProcedure(List<TKey> ids, ProcedureWriteKey key);
    void RemoveByProcedure(TKey id, ProcedureWriteKey key);
}


