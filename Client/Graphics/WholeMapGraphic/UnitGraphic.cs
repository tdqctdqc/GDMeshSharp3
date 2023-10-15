
using System;
using System.Collections.Concurrent;
using Godot;

public partial class UnitGraphic : MeshInstance2D
{
    private UnitGraphic() { }
    private int _currSegment = -1;

    public static UnitGraphic Construct(Unit unit, GraphicsSegmenter segmenter,
        Data d)
    {
        return new UnitGraphic(unit, segmenter, d);
    }
    protected UnitGraphic(Unit unit, GraphicsSegmenter segmenter, Data d)
    {
        Position = unit.Regime.Entity(d).Capital.Entity(d).Center;
        var m = new QuadMesh();
        m.Size = Vector2.One * 20f;
        Mesh = m;

        var subMesh = new QuadMesh();
        subMesh.Size = m.Size * .8f;
        var sub = new MeshInstance2D();
        sub.Mesh = subMesh;
        AddChild(sub);
        Modulate = unit.Regime.Entity(d).SecondaryColor;
        sub.Modulate = unit.Regime.Entity(d).PrimaryColor;
        segmenter.AddElement(this, unit.Position);
    }
    public void Update(Unit unit, Data data, GraphicsSegmenter segmenter,
        ConcurrentQueue<Action> queue)
    {
        queue.Enqueue(() => _currSegment = segmenter.SwitchSegments(this, unit.Position, _currSegment));
    }
    
    public override void _Process(double delta)
    {
    }
}