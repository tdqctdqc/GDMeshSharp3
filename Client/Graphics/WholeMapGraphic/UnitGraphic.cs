
using Godot;

public partial class UnitGraphic : Node2D
{
    public void Draw(Unit unit, Data data)
    {
        this.ClearChildren();
        var mb = new MeshBuilder();
        var regime = unit.Regime.Entity(data);
        var group = unit.GetGroup(data);
        var groupColor = group != null ? group.Color : Colors.White;
        var iconSize = 15f;
        mb.AddPoint(Vector2.Zero, iconSize, Colors.Black);
        mb.AddPoint(Vector2.Zero, iconSize - .2f, groupColor);
        mb.AddPoint(Vector2.Zero, iconSize * .8f, regime.GetUnitColor());
        AddChild(mb.GetMeshInstance());
    }
}