
using System.Linq;
using Godot;

public abstract partial class PolyCellBorder
    : Node2D, IMapChunkGraphicNode
{
    public MapChunk Chunk { get; private set; }
    public string Name { get; private set; }
    Node2D IMapChunkGraphicNode.Node => this;

    public PolyCellBorder(string name, MapChunk chunk, 
        Data data)
    {
        Chunk = chunk;
        Name = name;
        Draw(data);
    }
    private PolyCellBorder() : base()
    {
    }

    protected abstract bool InUnion(PolyCell p1, PolyCell p2, Data data);
    protected abstract float GetThickness(PolyCell p1, PolyCell p2, Data data);
    protected abstract Color GetColor(PolyCell p1, Data data);
    
    public void Draw(Data data)
    {
        this.ClearChildren();
        var mb = MeshBuilder.GetFromPool();
        var cells = Chunk.Polys.SelectMany(p => p.GetCells(data)).ToHashSet();
        foreach (var element in cells)
        {
            var color = GetColor(element, data);
            var offset = Chunk.RelTo.GetOffsetTo(element.RelTo, data);
            foreach (var n in element.GetNeighbors(data))
            {
                if (InUnion(n, element, data)) continue;
                mb.DrawPolyCellEdge(element, n, p => GetColor(p, data), GetThickness(element, n, data), Chunk.RelTo.Center, data);
            }
        }
        
        if (mb.Tris.Count == 0) return;
        AddChild(mb.GetMeshInstance());
        mb.Return();
    }
}