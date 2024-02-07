using System.Collections.Generic;
using System.Linq;

public class Frontline
{
    public List<FrontFace> Faces { get; private set; }
    public Frontline(List<FrontFace> faces)
    {
        Faces = faces;
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
}