using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ChunkChangeListener<TKey>
{
    public Dictionary<MapChunk, RefAction<TKey>> Added { get; private set; }
    public Dictionary<MapChunk, RefAction<TKey>> Changed { get; private set; }
    public Dictionary<MapChunk, RefAction<TKey>> Removed { get; private set; }

    public ChunkChangeListener(Data data)
    {
        Added = new Dictionary<MapChunk, RefAction<TKey>>();
        Changed = new Dictionary<MapChunk, RefAction<TKey>>();
        Removed = new Dictionary<MapChunk, RefAction<TKey>>();
        foreach (var chunk in data.Planet.PolygonAux.Chunks)
        {
            Added.Add(chunk, new RefAction<TKey>());
            Changed.Add(chunk, new RefAction<TKey>());
            Removed.Add(chunk, new RefAction<TKey>());
        }
    }

    public void MarkAdded(TKey key, MapChunk chunk)
    {
        Added[chunk].Invoke(key);
    }
    public void MarkRemoved(TKey key, MapChunk chunk)
    {
        Removed[chunk].Invoke(key);
    }
}

public static class ChunkChangeListenerExt
{
    public static void ListenForPolyEntity<TEntity, TKey>(
        this ChunkChangeListener<TKey> l,
        Data data, 
        Func<TEntity, TKey> getKey,
        Func<TEntity, MapPolygon> getPoly) where TEntity : Entity
    {
        var created = data.EntityTypeTree[typeof(TEntity)].Created;
        created.Subscribe(notice =>
        {
            foreach (var e in notice.Entities)
            {
                var v = (TEntity) e;
                var p = getPoly(v);
                var chunk = p.GetChunk(data);
                l.MarkAdded(getKey(v), chunk);
            }
        });
        var destroyed = data.EntityTypeTree[typeof(TEntity)].Destroyed;
        destroyed.Subscribe(notice =>
        {
            var v = (TEntity) notice.Entity;
            var p = getPoly(v);
            var chunk = p.GetChunk(data);
            l.MarkRemoved(getKey(v), chunk);
        });
        
        var es = data.GetRegister<TEntity>().Entities;
        var keys = es.Select(getKey);
        var chunks = es.Select(e => getPoly(e).GetChunk(data));
        var dic = es.ToDictionary(getKey, e => getPoly(e).GetChunk(data));
        foreach (var kvp in dic)
        {
            l.MarkAdded(kvp.Key, kvp.Value);
        }
    }
    public static void ListenForMultiPolyEntity<TEntity, TKey>(
        this ChunkChangeListener<TKey> l,
        Data data, 
        Func<TEntity, TKey> getKey,
        Func<TEntity, IEnumerable<MapPolygon>> getPolys) where TEntity : Entity
    {
        var created = data.EntityTypeTree[typeof(TEntity)].Created;
        created.Subscribe(notice =>
        {
            foreach (var e in notice.Entities)
            {
                var v = (TEntity) e;
                var ps = getPolys(v);
                foreach (var p in ps)
                {
                    var chunk = p.GetChunk(data);
                    l.MarkAdded(getKey(v), chunk);
                }
            }
        });
        var destroyed = data.EntityTypeTree[typeof(TEntity)].Destroyed;
        destroyed.Subscribe(notice =>
        {
            var v = (TEntity) notice.Entity;
            var ps = getPolys(v);
            foreach (var p in ps)
            {
                var chunk = p.GetChunk(data);
                l.MarkRemoved(getKey(v), chunk);
            }
        });
    }
    
    public static void ListenForChange<TKey, TEntity, TValue>(
        this ChunkChangeListener<TKey> l,
        Data data, ValChangeAction<TValue> changeTrigger, Func<TEntity, MapPolygon> getPoly,
        Func<TEntity, TKey> getKey)
        where TEntity : Entity
    {
        changeTrigger.Subscribe(n =>
        {
            var e = (TEntity) n.Entity;
            var poly = getPoly(e);
            var key = getKey(e);
            l.Changed[poly.GetChunk(data)].Invoke(key);
        });
    }
    public static void Listen<TKey, TValue>(
        this ChunkChangeListener<TKey> l,
        Data data, 
        Func<TValue, MapPolygon> getPoly, 
        Func<TValue, TKey> getKey,
        RefAction<TValue> createTrigger,
        RefAction<TValue> removeTrigger)
    {
        createTrigger.Subscribe(v =>
        {
            var chunk = getPoly(v).GetChunk(data);
            var key = getKey(v);
            l.MarkAdded(key, chunk);
        });
        removeTrigger.Subscribe(v =>
        {
            var chunk = getPoly(v).GetChunk(data);
            var key = getKey(v);
            l.MarkRemoved(getKey(v), chunk);
        });
    }
}
