
using Godot;

public class PolyCell
{
    public Vector2 RelTo { get; private set; }
    public Vector2[] RelBoundary { get; private set; }
    public ModelRef<Vegetation> Vegetation { get; private set; }
    public ModelRef<Landform> Landform { get; private set; }
    public PolyCell(Vector2 relTo, 
        Vector2[] relBoundary,
        ModelRef<Vegetation> vegetation,
        ModelRef<Landform> landform)
    {
        RelTo = relTo;
        RelBoundary = relBoundary;
        Landform = landform;
        Vegetation = vegetation;
    }
}