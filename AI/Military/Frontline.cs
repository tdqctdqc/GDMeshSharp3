using System.Collections.Generic;
using System.Linq;
using Godot;

public class Frontline
{
    public Regime Regime { get; private set; }
    public List<FrontFace> Faces { get; private set; }
    public List<List<FrontFace>> AdvanceLines { get; private set; }
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

        AdvanceLines = FrontFinder
            .FindFront(natives.Union(advanceInto).ToHashSet(),
                c =>
                    c.Controller.RefId != Regime.Id 
                    && c.Controller.IsEmpty() == false
                    && advanceInto.Contains(c) == false,
                d);
        

        // var boundaryCells = advanceInto
        //     .Where(c => c.GetNeighbors(d)
        //                     .Any(n => advanceInto.Contains(n) == false
        //                 && natives.Contains(n) == false)).ToArray();
        // var boundaryChains = boundaryCells
    }
}