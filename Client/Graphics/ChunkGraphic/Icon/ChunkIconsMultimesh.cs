
using System.Collections.Generic;
using Godot;

public abstract partial class ChunkIconsMultiMesh<TModel, TInstance> 
    : Node2D, IMapChunkGraphicNode
{
    public string Name { get; private set; }
    public MapChunk Chunk { get; private set; }
    public Vector2 IconDim { get; private set; }
    Node2D IMapChunkGraphicNode.Node => this;
    public Dictionary<TModel, MultiMeshInstance2D> MultiMeshes { get; private set; }
    
    public ChunkIconsMultiMesh(string name, MapChunk chunk, Vector2 iconDim)
    {
        Name = name;
        IconDim = iconDim;
        Chunk = chunk;
        MultiMeshes = new Dictionary<TModel, MultiMeshInstance2D>();
    }

    public void Draw(Data d)
    {
        Clear();
        var elementsByModel = GetElements(d)
            .SortInto(e => GetModel(e, d));
        foreach (var kvp in elementsByModel)
        {
            var model = kvp.Key;
            var elements = kvp.Value;
            var mm = MultiMeshes.GetOrAdd(model,
                model =>
                {
                    var mmi = new MultiMeshInstance2D();
                    mmi.Texture = GetTexture(model);
                    var quad = new QuadMesh();
                    quad.Size = IconDim;
                    mmi.Multimesh = new MultiMesh();
                    mmi.Multimesh.Mesh = quad;
                    AddChild(mmi);
                    return mmi;
                });
            mm.Multimesh.InstanceCount = elements.Count;
            for (var i = 0; i < elements.Count; i++)
            {
                var e = elements[i];
                var pos = Chunk.RelTo.GetOffsetTo(GetWorldPos(e, d), d);
                mm.Multimesh.SetInstanceTransform2D(i, new Transform2D(0f, new Vector2(1f, -1f), 0f, pos));
            }
        }
    }

    private void Clear()
    {
        foreach (var kvp in MultiMeshes)
        {
            kvp.Value.Multimesh.InstanceCount = 0;
        }
    }
    
    protected abstract Texture2D GetTexture(TModel t);
    protected abstract IEnumerable<TInstance> GetElements(Data d);
    protected abstract TModel GetModel(TInstance t, Data d);
    protected abstract Vector2 GetWorldPos(TInstance t, Data d);
}