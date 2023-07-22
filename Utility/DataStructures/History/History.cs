// using System;
// using System.Collections.Generic;
// using System.Linq;
// using MessagePack;
//
// public abstract class History<TElement, TData>
// {
//     public SnapshotHolder<TElement, TData> Snapshots { get; protected set; }
//     [SerializationConstructor] protected History(SnapshotHolder<TElement, TData> snapshots)
//     {
//         Snapshots = snapshots;
//     }
//     public void AddSnapshot(int tick, Snapshot<TElement, TData> snap, ProcedureWriteKey key)
//     {
//         Snapshots.Add(tick, snap);
//     }
//
//     public TData GetLatest(TElement el)
//     {
//         return Snapshots.GetLatest(el);
//     }
// }