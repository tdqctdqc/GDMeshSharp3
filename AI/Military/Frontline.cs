using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Frontline
{
    public Regime Regime { get; private set; }
    public List<FrontFace> Faces { get; private set; }
    public List<PolyCell[]> FaceAdvanceRoutes { get; private set; }
    public List<FrontFace> AdvanceFront { get; private set; }
    public List<List<FrontFace>> SalientFronts { get; private set; }
    public HashSet<PolyCell> AdvanceInto { get; private set; }
    public Frontline(List<FrontFace> faces, Regime regime)
    {
        Faces = faces;
        Regime = regime;
    }

    public bool CheckReunite(
        List<List<FrontFace>> frontLines,
        HashSet<FrontFace> frontFaces,
        HashSet<FrontFace> otherSegFaces,
        LogicWriteKey key,
        out List<List<FrontFace>> res)
    {
        var valid = Faces
            .Where(frontFaces.Contains).ToHashSet();
        var resInner = new List<List<FrontFace>>();
        for (var i = 0; i < frontLines.Count; i++)
        {
            var line = frontLines[i];
            line.DoForRuns(
                c =>
                {
                    return frontFaces.Contains(c)
                        && otherSegFaces.Contains(c) == false;
                },
                r =>
                {
                    //bc of 'single' edges !!
                    if (r.Any(valid.Contains))
                    {
                        var start = r.FindIndex(f => valid.Contains(f));
                        var end = r.FindLastIndex(f => valid.Contains(f));
                        var l = r.GetRange(start, end - start + 1);
                        resInner.Add(l);
                    }
                }
            );
        }

        res = resInner;
        if (res.Count == 1)
        {
            Faces = res.First();
        }
        else
        {
            Faces.Clear();
        }
        return res.Count() == 1;
    }

    public void SetAdvanceInto(
        HashSet<PolyCell> advanceInto, 
        Data d)
    {
        AdvanceInto = advanceInto;
        var natives = Faces
            .Select(f => f.GetNative(d)).ToHashSet();

        SalientFronts = FrontFinder
            .FindFront(advanceInto.Union(natives).ToHashSet(),
                c =>
                {
                    return c.Controller.RefId != Regime.Id
                        // && c.Controller.IsEmpty() == false
                        && advanceInto.Contains(c) == false;
                },
            d);
        return;

        if (SalientFronts.Count == 1)
        {
            AdvanceFront = SalientFronts[0];
            return;
        }
        var currIndex = 0;
        AdvanceFront = new List<FrontFace>();
        
        while (currIndex < Faces.Count && currIndex != -1)
        {
            var curr = Faces[currIndex];
            var salientIndex = SalientFronts.FindIndex(f => f[0].JoinsWith(curr));

            if (salientIndex == -1)
            {
                AdvanceFront.Add(curr);
                currIndex++;
            }
            else
            {
                var salient = SalientFronts[salientIndex];
                AdvanceFront.AddRange(salient);
                currIndex = Faces.FindLastIndex(f => f.JoinsWith(salient[^1]));
            }
        }
    }
}