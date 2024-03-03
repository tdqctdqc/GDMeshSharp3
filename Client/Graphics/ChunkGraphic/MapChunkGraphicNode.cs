// using System;
// using System.Collections.Concurrent;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
// using Godot;
//
// public abstract partial class ChunkGraphic<TKey> 
//     : Node2D, IChunkGraphic
// {
//     public MapChunk Chunk { get; private set; }
//     public string Name { get; private set; }
//     private Dictionary<TKey, Node2D> _graphics;
//     Node2D IChunkGraphic.Node => this;
//     public ChunkGraphic(string name, Data data, MapChunk chunk)
//     {
//         Name = name;
//         Chunk = chunk;
//         _graphics = new Dictionary<TKey, Node2D>();
//
//         var keys = GetKeys(data);
//         foreach (var key in keys)
//         {
//             Add(key, data);
//         }
//     }
//     protected ChunkGraphic() { }
//     public override void _ExitTree()
//     {
//     }
//
//
//     public void Update(Data d, ConcurrentQueue<Action> queue)
//     {
//         
//     }
//     public void QueueAdd(TKey key)
//     {
//         _queuedToAdd.Add(key);
//     }
//     public void QueueRemove(TKey key)
//     {
//         _queuedToRemove.Add(key);
//     }
//     public void QueueChange(TKey key)
//     {
//         _queuedToChange.Add(key);
//     }
//
//     public void QueueChangeAll()
//     {
//         _queuedToChange.AddRange(_graphics.Keys);
//     }
//     private void Add(TKey key, Data data)
//     {
//         if (Ignore(key, data)) return;
//         var graphic = MakeGraphic(key, data);
//         _graphics.Add(key, graphic);
//         this.AddChild(graphic);
//     }
//     private void Change(TKey key, Data data)
//     {
//         if (_graphics.ContainsKey(key))
//         {
//             _graphics[key].QueueFree();
//             _graphics.Remove(key);
//         }
//         Add(key, data);
//     }
//     private void Remove(TKey key, Data data)
//     {
//         if (_graphics.ContainsKey(key) == false) return;
//         var graphic = _graphics[key];
//         graphic.QueueFree();
//         _graphics.Remove(key);
//     }
//     
//     protected void SetRelPos(Node2D node, MapPolygon poly, Data data)
//     {
//         node.Position = Chunk.RelTo.GetOffsetTo(poly, data);
//     }
//     protected void SetRelPos(Node2D node, Vector2 pos, Data data)
//     {
//         node.Position = Chunk.RelTo.GetOffsetTo(pos, data);
//     }
//     protected abstract Node2D MakeGraphic(TKey element, Data data);
//     protected abstract IEnumerable<TKey> GetKeys(Data data);
//     protected abstract bool Ignore(TKey element, Data data);
// }
//
// public interface IChunkGraphic
// {
//     string Name { get; }
//     Node2D Node { get; }
// }
//
