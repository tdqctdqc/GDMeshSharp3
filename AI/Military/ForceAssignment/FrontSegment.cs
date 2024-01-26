using System.Collections.Generic;
using System.Linq;

public class FrontSegment
{
    public List<FrontFace<PolyCell>> Faces { get; private set; }
    public FrontSegment(List<FrontFace<PolyCell>> faces)
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
        var valid = Faces.Where(frontFaces.Contains).ToHashSet();
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
                    if (r.Any(valid.Contains))
                    {
                        resInner.Add(r);
                    }
                }
            );
        }

        res = resInner;
        if (res.Count == 1)
        {
            Faces = res.First();
        }
        return res.Count() == 1;
    }
}