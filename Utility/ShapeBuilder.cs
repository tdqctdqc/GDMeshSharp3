
using Godot;

public static class ShapeBuilder
{
    public static Vector2[] GetArrow(Vector2 from, Vector2 to,
        float thickness)
    {
        var length = from.DistanceTo(to);
        var arrowLength = Mathf.Min(length / 2f, thickness * 1.5f);
        var stemLength = length - arrowLength;

        var axis = (to - from).Normalized();
        var orth = axis.Orthogonal();
        var arrowBase = from + axis * stemLength;

        return new[]
        {
            from + orth * thickness / 2f,
            arrowBase + orth * thickness / 2f,
            arrowBase + orth * thickness,
            to,
            arrowBase - orth * thickness,
            arrowBase - orth * thickness / 2f,
            from - orth * thickness / 2f
        };
    }
}