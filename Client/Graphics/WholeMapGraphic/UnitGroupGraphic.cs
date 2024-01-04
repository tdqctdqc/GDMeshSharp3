
using System;
using System.Collections.Concurrent;
using Godot;

public partial class UnitGroupGraphic : Node2D
{
    public UnitGroup Group { get; private set; }
    public MeshInstance2D Child { get; private set; }
    private static float _groupIconSize = 10f;
    private static float _unitIconSize = 5f;
    private UnitGroupGraphic() { }
    public UnitGroupGraphic(UnitGroup unitGroup, GraphicsSegmenter segmenter, Data d)
    {
        Group = unitGroup;
    }

    public void Draw(Data data, GraphicsSegmenter segmenter)
    {
        if (Child != null)
        {
            Child.QueueFree();
            Child = null;
        }
        var regime = Group.Regime.Entity(data);
        var m = new QuadMesh();
        var mb = new MeshBuilder();

        var groupPos = Group.GetPosition(data);
        var darkened = regime.PrimaryColor.Darkened(.5f);
        // mb.AddPointMarker(Vector2.Zero, _groupIconSize, darkened);
        // mb.AddPointMarker(Vector2.Zero, _groupIconSize * .8f, regime.PrimaryColor);
        
        foreach (var unit in Group.Units.Items(data))
        {
            var relPos = groupPos.GetOffsetTo(unit.Position.Pos, data);
            mb.AddPoint(relPos, _unitIconSize, darkened);
            mb.AddPoint(relPos, _unitIconSize * .8f, regime.PrimaryColor);
        }

        Child = mb.GetMeshInstance();
        AddChild(Child);
        segmenter.SwitchSegments(this, groupPos);
    }
    public void Update(UnitGroup unit, Data data, GraphicsSegmenter segmenter,
        ConcurrentQueue<Action> queue)
    {
        queue.Enqueue(() =>
        {
            Draw(data, segmenter);
        });
    }
    
    public override void _Process(double delta)
    {
    }
}