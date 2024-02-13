using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class MilAiMemo
{
    public HashSet<ERef<UnitGroup>> FrontSegmentGroups { get; private set; }
    public MilAiMemo(Regime owner, Data d)
    {
        var ai = d.HostLogicData.RegimeAis[owner];
        FrontSegmentGroups = new HashSet<ERef<UnitGroup>>();

        var root = ai.Military.Deployment.GetRoot();
        if (root == null)
        {
            return;
        }
        var segments = root.GetDescendentsOfType<FrontSegment>().ToArray();
        foreach (var seg in segments)
        {
            FrontSegmentGroups.AddRange(seg.HoldLine.Groups);
        }
    }
    

    public void Finish(DeploymentAi ai, DeploymentRoot root, LogicWriteKey key)
    {
        var d = key.Data;
        var theaterSegs = new Dictionary<Theater, FrontSegment[]>();
        foreach (var theater in root.GetDescendentsOfType<Theater>())
        {
            theaterSegs.Add(theater, theater.GetDescendentsOfType<FrontSegment>().ToArray());
        }
        var validGroups = FrontSegmentGroups.Where(g => d.HasEntity(g.RefId)).ToArray();
        foreach (var g in validGroups)
        {
            var group = g.Entity(key.Data);
            var groupCell = group.GetCell(d);
            if (groupCell.Controller.RefId != ai.Regime.Id)
            {
                throw new Exception();
            }

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
            if (frontSegment != null)
            {
                frontSegment.HoldLine.PushGroup(ai, group, key);
            }
        }
    }
}