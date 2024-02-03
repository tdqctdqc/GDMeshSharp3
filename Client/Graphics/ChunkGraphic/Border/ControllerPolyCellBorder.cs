
using Godot;

public partial class ControllerPolyCellBorder : PolyCellBorder
{
    public ControllerPolyCellBorder(MapChunk chunk, 
        Data data) : base("Controller border", chunk, data)
    {
    }

    protected override bool InUnion(PolyCell p1, PolyCell p2, Data data)
    {
        return p1.Controller.RefId == p2.Controller.RefId;
    }

    protected override float GetThickness(PolyCell p1, PolyCell p2, Data data)
    {
        return 5f;
    }

    protected override Color GetColor(PolyCell p1, Data data)
    {
        return p1.Controller.IsEmpty() ? Colors.Transparent : p1.Controller.Entity(data).SecondaryColor;
    }
}