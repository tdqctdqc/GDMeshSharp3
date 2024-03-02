
using Godot;

public partial class ControllerPolyCellBorder : PolyCellBorder
{
    public ControllerPolyCellBorder(MapChunk chunk, 
        Data data) : base("Controller border", chunk, data)
    {
    }

    protected override bool InUnion(Cell p1, Cell p2, Data data)
    {
        return p1.Controller.RefId == p2.Controller.RefId;
    }

    protected override float GetThickness(Cell p1, Cell p2, Data data)
    {
        return 2f;
    }

    protected override Color GetColor(Cell p1, Data data)
    {
        return p1.Controller.IsEmpty() ? Colors.Transparent : p1.Controller.Entity(data).SecondaryColor;
    }
}