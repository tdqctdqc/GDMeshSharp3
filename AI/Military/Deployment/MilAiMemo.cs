using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class MilAiMemo
{
    public HashSet<UnitGroup> FrontSegmentGroups { get; private set; }
    public MilAiMemo(Regime owner, Data d)
    {
        var ai = d.HostLogicData.RegimeAis[owner];
        FrontSegmentGroups = new HashSet<UnitGroup>();

        var root = ai.Military.Deployment.GetRoot();
        if (root == null)
        {
            return;
        }
        var segments = root.GetDescendentAssignmentsOfType<HoldLineAssignment>().ToArray();
        foreach (var seg in segments)
        {
            FrontSegmentGroups.AddRange(seg.Groups);
        }
    }
    

    public void Finish(DeploymentAi ai, DeploymentRoot root, LogicWriteKey key)
    {
        var d = key.Data;
        var theaterSegs = new Dictionary<Theater, HoldLineAssignment[]>();
        foreach (var theater in root.SubBranches.OfType<Theater>())
        {
            theaterSegs.Add(theater, theater.GetDescendentAssignmentsOfType<HoldLineAssignment>().ToArray());
        }
        var validGroups = FrontSegmentGroups.Where(g => d.HasEntity(g.Id)).ToArray();
        foreach (var group in validGroups)
        {
            var groupCell = group.GetCell(d);

            if (groupCell is LandCell == false)
            {
                throw new Exception();
            }
            var (theater, segments) = theaterSegs
                .First(kvp => kvp.Key.HeldCellIds.Contains(groupCell.Id));
            if (segments.Length == 0)
            {
                continue;
            }
            var frontSegment = segments
                .FirstOrDefault(s => s.Frontline.Faces
                    .Any(f => f.Native == groupCell.Id));
            if (frontSegment == null)
            {
                frontSegment = segments.MinBy(s =>
                    s.Frontline.Faces.First().GetNative(d)
                        .GetCenter()
                        .GetOffsetTo(group.GetCell(d).GetCenter(), d)
                        .Length());
            }
            if (frontSegment != null)
            {
                frontSegment.PushGroup(ai, group, key);
            }
        }
    }
}