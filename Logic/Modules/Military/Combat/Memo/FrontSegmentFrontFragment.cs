
using System.Collections.Generic;
using Godot;

public class FrontSegmentFrontFragment : IFrontFragment
{
    public Vector2I LineGroupsRange { get; set; }
    public Vector2I ReserveGroupsRange { get; set; }
    public Vector2I InsertGroupsRange { get; set; }
    public Vector2I FacesRange { get; set; }
    
    public FrontlineMemo FrontSegment { get; private set; }
    public FrontSegmentFrontFragment(FrontlineMemo frontSegment, Vector2I lineGroupsRange, Vector2I facesRange)
    {
        FrontSegment = frontSegment;
        LineGroupsRange = lineGroupsRange;
        FacesRange = facesRange;
    }

    public int GetLength()
    {
        return FacesRange.Y - FacesRange.X + 1;
    }

    public List<FrontFace> GetFaces()
    {
        return FrontSegment.Faces.GetRange(FacesRange.X, FacesRange.Y - FacesRange.X + 1);
    }

    public void SetRange(float startRatio, float endRatio)
    {
        LineGroupsRange = FrontSegment.LineGroups.GetProportionIndicesOfList(startRatio, endRatio);
        ReserveGroupsRange = FrontSegment.ReserveGroups.GetProportionIndicesOfList(startRatio, endRatio);
        InsertGroupsRange = FrontSegment.InsertGroups.GetProportionIndicesOfList(startRatio, endRatio);
    }
    public (List<UnitGroup> lineGroups, List<UnitGroup> reserveGroups,
        List<UnitGroup> insertGroups) GetGroups()
    {
        var line = LineGroupsRange == -Vector2I.One
            ? new List<UnitGroup>()
                : FrontSegment.LineGroups.GetRange(LineGroupsRange.X, LineGroupsRange.Y - LineGroupsRange.X + 1);
        var reserve = ReserveGroupsRange == -Vector2I.One
            ? new List<UnitGroup>()
            : FrontSegment.ReserveGroups.GetRange(ReserveGroupsRange.X, ReserveGroupsRange.Y - ReserveGroupsRange.X + 1);
        var insert = InsertGroupsRange == -Vector2I.One
            ? new List<UnitGroup>()
            : FrontSegment.ReserveGroups.GetRange(InsertGroupsRange.X, InsertGroupsRange.Y - InsertGroupsRange.X + 1);
        return (line, reserve, insert);
    }
    public FrontFace GetFirst()
    {
        return FrontSegment.Faces[FacesRange.X];
    }

    public FrontFace GetLast()
    {
        return FrontSegment.Faces[FacesRange.Y];
    }
}