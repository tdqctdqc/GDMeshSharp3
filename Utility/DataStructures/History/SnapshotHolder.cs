using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class SnapshotHolder<TElement, TData>
{
    private int _latest = 0;

    public Dictionary<int, Snapshot<TElement, TData>> Snapshots { get; private set; }

    public static SnapshotHolder<TElement, TData> Construct()
    {
        return new SnapshotHolder<TElement, TData>(new Dictionary<int, Snapshot<TElement, TData>>());
    }
    [SerializationConstructor] public SnapshotHolder(Dictionary<int, Snapshot<TElement, TData>> snapshots)
    {
        Snapshots = snapshots;
    }

    public void Add(int tick, Snapshot<TElement, TData> snap)
    {
        if (_latest < tick) _latest = tick;
        Snapshots.Add(tick, snap);
    }
    public TData GetLatest(TElement el)
    {
        if (Snapshots[_latest].Entries.ContainsKey(el) == false) return default;
        return Snapshots[_latest].Entries[el];
    }
}
