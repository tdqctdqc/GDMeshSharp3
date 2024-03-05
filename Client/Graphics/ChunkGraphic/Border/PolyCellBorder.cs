
using System;
using System.Linq;
using Godot;

public abstract partial class PolyCellBorder
    : Node2D, IChunkGraphicModule
{
    public MapChunk Chunk { get; private set; }
    public string Name { get; private set; }
    public Node2D Node => this;
    public PolyCellBorder(string name, MapChunk chunk, 
        LayerOrder layerOrder, Data data)
    {
        Chunk = chunk;
        Name = name;
        ZAsRelative = false;
        ZIndex = (int)layerOrder;
    }
    private PolyCellBorder() : base()
    {
    }
    protected abstract bool InUnion(Cell p1, Cell p2, Data data);
    protected abstract float GetThickness(Cell m, Cell n, Data data);
    protected abstract Color GetColor(Cell p1, Data data);
    public abstract void RegisterForRedraws(Data d);
    public abstract void DoUiTick(UiTickContext context, Data d);

    public void Draw(Data data)
    {
        this.ClearChildren();
        var mb = MeshBuilder.GetFromPool();
        var cells = Chunk.Polys
            .SelectMany(p => p.GetCells(data)).ToHashSet();
        foreach (var cell in cells)
        {
            if (cell is LandCell l == false) continue;
            var cellColor = GetColor(cell, data);
            foreach (var nCell in cell.GetNeighbors(data))
            {
                if (nCell is LandCell lN == false) continue;
                if (InUnion(cell, nCell, data)) continue;
                if (InUnion(nCell, cell, data)) continue;
                mb.DrawPolyCellEdge(l, lN, 
                    p => cellColor, 
                    GetThickness(cell, nCell, data), 
                    Chunk.RelTo.Center, data);
            }
        }
        
        if (mb.TriVertices.Count == 0) return;
        AddChild(mb.GetMeshInstance());
        mb.Return();
    }
}