
using System;
using System.Collections.Concurrent;
using Godot;

public partial class UnitGraphic : MeshInstance2D
{
    private UnitGraphic() { }
    public UnitGraphic(Unit unit, GraphicsSegmenter segmenter, Data d)
    {
        var m = new QuadMesh();
        m.Size = Vector2.One * 20f;
        Mesh = m;
        var r = unit.Regime.Entity(d);
        var subMesh = new QuadMesh();
        subMesh.Size = m.Size * .8f;
        var sub = new MeshInstance2D();
        sub.Mesh = subMesh;
        AddChild(sub);
        Modulate = r.SecondaryColor;
        sub.Modulate = r.PrimaryColor;
        segmenter.AddElement(this, unit.Position);
    }
    public void Update(Unit unit, Data data, GraphicsSegmenter segmenter,
        ConcurrentQueue<Action> queue)
    {
        queue.Enqueue(() =>
        {
            segmenter.SwitchSegments(this, unit.Position);
        });
    }
    
    public override void _Process(double delta)
    {
    }
}