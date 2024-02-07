using System;
using System.Linq;
using System.Collections.Generic;
using Godot;

public class MilAiMemo
{
    public Regime Owner { get; private set; }
    public HashSet<TheaterMemo> Theaters { get; private set; }
    public HashSet<FrontlineMemo> Frontlines { get; private set; }
    public MilAiMemo(Regime owner, Data d)
    {
        Owner = owner;
        var ai = d.HostLogicData.RegimeAis[owner].Military.Deployment;
        
        Theaters = ai.Root.GetChildrenOfType<Theater>()
            .Select(t => new TheaterMemo(t, d))
            .ToHashSet();
        Frontlines = new HashSet<FrontlineMemo>();
        var frontSegs = ai.Root
            .GetChildrenOfType<FrontSegment>().ToArray();
        foreach (var frontSegment in frontSegs)
        {
            Frontlines.Add(new FrontlineMemo(frontSegment, d));
        }
    }
    public void AddCell(PolyCell cell, Data d)
    {
        var theaters = cell.GetNeighbors(d)
            .Where(n => n.Controller.RefId == Owner.Id)
            .Select(n => Theaters.First(t => t.Cells.Contains(n)))
            .Distinct()
            .ToArray();
        if (theaters.Length == 0)
        {
            var newTheater = new TheaterMemo(new HashSet<PolyCell> { cell },
                new HashSet<UnitGroup>(), new HashSet<DeploymentBranch>());
            Theaters.Add(newTheater);
        }
        else if (theaters.Length == 1)
        {
            theaters[0].Cells.Add(cell);
        }
        else
        {
            Theaters.ExceptWith(theaters);
            var newCells = theaters
                .SelectMany(t => t.Cells)
                .ToHashSet();
            var newChildren = theaters
                .SelectMany(t => t.Children)
                .ToHashSet();
            var newReserves = theaters.SelectMany(t => t.Reserves).ToHashSet();
            
            newCells.Add(cell);
            var newTheater = new TheaterMemo(newCells, newReserves, newChildren);
            Theaters.Add(newTheater);
        }
        Runs(cell, d);
    }

    
    public void RemoveCell(PolyCell cell, Data d)
    {
        var theater = Theaters.First(t => t.Cells.Contains(cell));
        theater.Cells.Remove(cell);
        if (theater.Cells.Count == 0)
        {
            Theaters.Remove(theater);
        }
        else
        {
            var unions = UnionFind.Find(theater.Cells,
                (c, d) => true, c => c.GetNeighbors(d));
            if (unions.Count() > 1)
            {
                Theaters.Remove(theater);
                foreach (var union in unions)
                {
                    var h = union.ToHashSet();
                    var assignments = theater.Children
                        .Where(b => h.Contains(b.GetCharacteristicCell(d)))
                        .ToHashSet();
                    var reserves = theater.Reserves
                        .Where(g => h.Contains(g.GetCell(d)))
                        .ToHashSet();
                    var newTheater = new TheaterMemo(union.ToHashSet(),
                        reserves, assignments);
                    Theaters.Add(newTheater);
                }
            }
        }
        
        Runs(cell, d);
    }
    
    private void Runs(PolyCell cell, Data d)
    {
        var frontFragments = new Dictionary<FrontlineMemo, List<FrontSegmentFrontFragment>>();
        var res = new HashSet<Frontline>();
        var affected = Frontlines
            .Where(fl => fl.Faces.Any(f => f.Native == cell.Id || f.Foreign == cell.Id))
            .ToArray();
        Frontlines.ExceptWith(affected);
        foreach (var frontline in affected)
        {
            var thisFlFragments = new List<FrontSegmentFrontFragment>();
            frontline.Faces.DoForRunIndices(
                f => f.Native != cell.Id
                     && f.Foreign != cell.Id,
                l =>
                {
                    thisFlFragments.Add(new FrontSegmentFrontFragment(frontline, -Vector2I.One, l));
                });
            if (thisFlFragments.Count == 0)
            {
                //handle deletion 
                continue;
            }
            var totalFragsLength = thisFlFragments.Sum(fl => fl.GetLength());
            var runningTotal = 0;
            for (var i = 0; i < thisFlFragments.Count; i++)
            {
                var l = thisFlFragments[i];
                var startRatio = (float)runningTotal / totalFragsLength;
                runningTotal += l.GetLength();
                var endRatio = (float)runningTotal / totalFragsLength;
                l.SetRange(startRatio, endRatio);
            }
            frontFragments.Add(frontline, thisFlFragments);
        }
        
        var newFaces = cell.GetNeighbors(d).Where(
            n => (n.Controller.RefId == Owner.Id) 
                 != (cell.Controller.RefId == Owner.Id)
        ).Select(n =>
        {
            if (n.Controller.RefId == Owner.Id) return FrontFace.Construct(n, cell, d);
            return FrontFace.Construct(cell, n, d);
        }).ToHashSet();
        var patches = new List<PatchFrontFragment>();
        while (newFaces.Count > 0)
        {
            var face = newFaces.First();
            var patch = new PatchFrontFragment(face, newFaces, d);
            patches.Add(patch);
        }
        
        var firsts = new Dictionary<FrontFace, IFrontFragment>();
        var lasts = new Dictionary<FrontFace, IFrontFragment>();
        foreach (var kvp1 in frontFragments)
        {
            foreach (var fragment in kvp1.Value)
            {
                firsts.Add(fragment.GetFirst(), fragment);
                lasts.Add(fragment.GetLast(), fragment);
            }
        }
        foreach (var patch in patches)
        {
            firsts.Add(patch.GetFirst(), patch);
            lasts.Add(patch.GetLast(), patch);
        }

        var fragments = firsts.Values
            .Union(lasts.Values).ToHashSet();
        while (fragments.Count > 0)
        {
            var fragment = fragments.First();
            var newFront = fragment.GetFrontLeftToRight(
                f => f.GetFirst().Left != null 
                    && lasts.ContainsKey(f.GetFirst().GetLeftNeighbor(d)),
                f => lasts[f.GetFirst().GetLeftNeighbor(d)],
                f => f.GetLast().Right != null &&
                    firsts.ContainsKey(f.GetLast().GetRightNeighbor(d)),
                f => firsts[f.GetLast().GetRightNeighbor(d)],
                f => true);
            var faces = newFront.SelectMany(f => f.GetFaces()).ToList();
            var groups = newFront.Select(f => f.GetGroups()).ToList();
            var fl = new FrontlineMemo(groups.SelectMany(g => g.lineGroups).ToList(),
                groups.SelectMany(g => g.reserveGroups).ToList(),
                groups.SelectMany(g => g.insertGroups).ToList(),
                faces);
            Frontlines.Add(fl);
            foreach (var taken in newFront)
            {
                remove(taken);
            }
            void remove(IFrontFragment f)
            {
                fragments.Remove(f);
                lasts.Remove(f.GetLast());
                firsts.Remove(f.GetFirst());
            }
        }
    }

    public void Finish(LogicWriteKey key)
    {
        var d = key.Data;
        var ai = d.HostLogicData.RegimeAis[Owner]
            .Military.Deployment;
        //disconnect any listeners
        foreach (var memo in Theaters)
        {
            var theater = Theater.Construct(ai, Owner,
                memo.Cells, key);
            ai.AddNode(theater);
            theater.SetParent(ai, ai.Root, key);
        }

        var theaters = ai.Root
            .GetChildrenOfType<Theater>().ToArray();
        foreach (var memo in Frontlines)
        {
            var frontSegment = FrontSegment.Construct(ai,
                Owner.MakeRef(), memo.Faces, false, key);
            var theater = theaters.First(t => t.HeldCellIds.Contains(memo.Faces.First().Native));
            ai.AddNode(frontSegment);
            frontSegment.SetParent(ai, theater, key);
            foreach (var g in memo.LineGroups)
            {
                frontSegment.HoldLine.AddGroup(ai, g, key.Data);
            }
            foreach (var g in memo.ReserveGroups)
            {
                frontSegment.Reserve.AddGroup(ai, g, key.Data);
            }
            foreach (var g in memo.InsertGroups)
            {
                frontSegment.Insert.AddGroup(ai, g, key.Data);
            }
        }
    }
}