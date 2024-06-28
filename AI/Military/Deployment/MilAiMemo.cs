using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class MilAiMemo
{
    public HashSet<Army> FrontSegmentGroups { get; private set; }
    public MilAiMemo(Alliance owner, Data d)
    {
        var ai = d.HostLogicData.AllianceAis[owner];
        FrontSegmentGroups = new HashSet<Army>();

        var root = ai.Military.Deployment.GetRoot();
        if (root == null)
        {
            return;
        }
        var segments = root.GetDescendentAssignmentsOfType<FrontlineAssignment>().ToArray();
        foreach (var seg in segments)
        {
            FrontSegmentGroups.AddRange(seg.Groups);
        }
    }
    public void Finish(DeploymentAi ai, DeploymentRoot root, LogicWriteKey key)
    {
        var d = key.Data;
        var theaterSegs = new Dictionary<TheaterBranch, FrontlineAssignment[]>();
        foreach (var theater in root.SubBranches.OfType<TheaterBranch>())
        {
            theaterSegs.Add(theater, theater.GetDescendentAssignmentsOfType<FrontlineAssignment>().ToArray());
        }
        var validGroups = FrontSegmentGroups.Where(g => d.HasEntity(g.Id)).ToArray();
        foreach (var group in validGroups)
        {
            var groupCell = group.GetHomeCell(d);

            if (groupCell is LandCell == false)
            {
                throw new Exception();
            }

            var theater = 
                theaterSegs.Keys.FirstOrDefault(
                v => v.Theater.Cells.Contains(groupCell));
            if (theater == null)
            {
                theater = theaterSegs.Keys.MinBy(t => t.GetCharacteristicCell(d)
                    .GetCenter().Offset(groupCell.GetCenter(), d).Length());
            }

            var segments = theaterSegs[theater];
            
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
                        .Offset(group.GetHomeCell(d).GetCenter(), d)
                        .Length());
            }
            if (frontSegment != null)
            {
                frontSegment.PushGroup(ai, group, key);
            }
        }
    }
}