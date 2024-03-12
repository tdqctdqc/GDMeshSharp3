
using Godot;

public partial class ControllerPolyCellBorder : PolyCellBorder
{
    public ControllerPolyCellBorder(MapChunk chunk, 
        Data data) : base("Controller border",
        chunk,
        new Vector2(0f, 1f),
        LayerOrder.PolyFill, data)
    {
    }

    protected override bool InUnion(Cell p1, Cell p2, Data data)
    {
        return p1.Controller.RefId == p2.Controller.RefId;
    }

    protected override float GetThickness(Cell m, Cell n, Data data)
    {
        return 2f;
    }

    protected override Color GetColor(Cell p1, Data data)
    {
        return p1.Controller.IsEmpty() ? Colors.Transparent : p1.Controller.Get(data).SecondaryColor;
    }

    public override void RegisterForRedraws(Data d)
    {
        this.RegisterDrawOnTick(d);
    }
}