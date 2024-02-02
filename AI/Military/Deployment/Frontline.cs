using System.Collections.Generic;
using System.Linq;

public class Frontline
{
    public List<FrontFace<PolyCell>> Faces { get; private set; }
    public Frontline(List<FrontFace<PolyCell>> faces)
    {
        Faces = faces;
    }

    public bool CheckReunite(
        List<List<FrontFace<PolyCell>>> frontLines,
        HashSet<FrontFace<PolyCell>> frontFaces,
        HashSet<FrontFace<PolyCell>> otherSegFaces,
        LogicWriteKey key,
        out List<List<FrontFace<PolyCell>>> res)
    {
        var valid = Faces
            .Where(frontFaces.Contains).ToHashSet();
        var resInner = new List<List<FrontFace<PolyCell>>>();
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
}