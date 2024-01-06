
using System.Collections.Generic;
using Godot;

public class LineAssignment
{
    public List<Vector2> SubLine { get; private set; }
    public float FromProportion { get; private set; }
    public float ToProportion { get; private set; }

    public LineAssignment(List<Vector2> subLine, float fromProportion, float toProportion)
    {
        SubLine = subLine;
        FromProportion = fromProportion;
        ToProportion = toProportion;
    }
}