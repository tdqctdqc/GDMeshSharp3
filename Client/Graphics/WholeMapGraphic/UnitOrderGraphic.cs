
using System;
using System.Collections.Concurrent;
using Godot;

public partial class UnitOrderGraphic : Node2D
{
    public UnitGroup Group { get; private set; }
    public MeshInstance2D Mesh { get; private set; }
    public UnitOrderGraphic(UnitGroup group, GraphicsSegmenter segmenter,
        Data d)
    {
        Group = group;
        Draw(d, segmenter);
    }

    private UnitOrderGraphic()
    {
        
    }

    public void Update(Data d, GraphicsSegmenter segmenter,
        ConcurrentQueue<Action> queue)
    {
        queue.Enqueue(() =>
        {
            Draw(d, segmenter);
        });
    }
    private void Draw(Data d, GraphicsSegmenter segmenter)
    {
        if (Mesh != null)
        {
            Mesh.QueueFree();
            Mesh = null;
        }
        segmenter.AddElement(this, Group.GetPosition(d));
        
        var mb = MeshBuilder.GetFromPool();
        var order = Group.GroupOrder;
        order.Draw(Group, Group.GetPosition(d), mb, d);
        if (mb.TriVertices.Count > 0)
        {
            Mesh = mb.GetMeshInstance();
            AddChild(Mesh);
        }
        mb.Return();
    }
}