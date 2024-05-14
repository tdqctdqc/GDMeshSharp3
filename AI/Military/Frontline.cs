using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class Frontline
{
    public Alliance Alliance { get; private set; }
    public List<FrontFace> Faces { get; private set; }
    public List<FrontFace> AdvanceFront { get; private set; }
    public HashSet<Cell> AdvanceInto { get; private set; }
    public Frontline(List<FrontFace> faces, 
        Alliance alliance)
    {
        Faces = faces;
        Alliance = alliance;
    }

    public void SetAdvanceInto(
        HashSet<Cell> advanceInto, 
        Data d)
    {
        AdvanceInto = advanceInto;
        var natives = Faces
            .Select(f => f.GetNative(d)).ToHashSet();

        var salients = FrontFinder
            .FindFront(advanceInto.Union(natives).ToHashSet(),
                c =>
                {
                    return Alliance.Members.RefIds.Contains(c.Controller.RefId) == false
                        && c is not RiverCell
                        && advanceInto.Contains(c) == false;
                },
            d);


        if (salients.Count == 1)
        {
            AdvanceFront = salients[0];
            return;
        }
        
        var currIndex = 0;

        AdvanceFront = new List<FrontFace>();
        while (currIndex < Faces.Count && currIndex != -1)
        {
            var curr = Faces[currIndex];
            var salientIndex = salients.FindIndex(f => f[0].JoinsWith(curr));

            if (salientIndex == -1)
            {
                AdvanceFront.Add(curr);
                currIndex++;
            }
            else
            {
                var salient = salients[salientIndex];
                AdvanceFront.AddRange(salient);
                currIndex = Faces.FindLastIndex(f => f.JoinsWith(salient[^1]));
            }
        }

    }
}