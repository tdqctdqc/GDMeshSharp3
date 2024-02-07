
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FrontlineMemo
{
    public List<UnitGroup> LineGroups { get; set; }
    public List<UnitGroup> ReserveGroups { get; set; }
    public List<UnitGroup> InsertGroups { get; set; }
    public List<FrontFace> Faces { get; set; }

    public FrontlineMemo(FrontSegment frontSegment, Data d)
    {
        Faces = frontSegment.Frontline.Faces.ToList();
        var first = frontSegment.Frontline.Faces.First();
        var last = frontSegment.Frontline.Faces.Last();
        var firstMid = first.GetMid(d);
        var axis = firstMid.GetOffsetTo(last.GetMid(d), d);
        ReserveGroups = frontSegment.Reserve.Groups
            .Select(g => g.Entity(d))
            .OrderBy(g => firstMid.GetOffsetTo(g.GetPosition(d), d).SignedProjectionLength(axis))
            .ToList();
        InsertGroups = frontSegment.Insert.Groups
            .Select(g => g.Entity(d))
            .OrderBy(g => firstMid.GetOffsetTo(g.GetPosition(d), d).SignedProjectionLength(axis))
            .ToList();
        LineGroups = frontSegment.HoldLine.GetGroupsInOrder(frontSegment, d);
    }

    public FrontlineMemo(List<UnitGroup> lineGroups, 
        List<UnitGroup> reserveGroups,
        List<UnitGroup> insertGroups,
        List<FrontFace> faces)
    {
        LineGroups = lineGroups;
        ReserveGroups = reserveGroups;
        InsertGroups = insertGroups;
        Faces = faces;
    }
}