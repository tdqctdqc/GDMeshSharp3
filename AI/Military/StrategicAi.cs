
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class StrategicAi
{
    public StrategicContext PrevContext { get; private set; }

    public Dictionary<ERef<Frontline>, HashSet<ERef<Frontline>>>
        FrontlineMerges { get; private set; }

    public Dictionary<ERef<Frontline>, Frontline>
        FrontlineCache { get; private set; }

    public StrategicContext Context { get; private set; }

    public static StrategicAi Construct(Alliance a, Data d)
    {
        return new StrategicAi(null,
            new Dictionary<ERef<Frontline>, HashSet<ERef<Frontline>>>());
    }

    [SerializationConstructor]
    private StrategicAi(
        StrategicContext prevContext,
        Dictionary<ERef<Frontline>, HashSet<ERef<Frontline>>> frontlineMerges)
    {
        // FrontlineMerges = frontlineMerges;
        PrevContext = prevContext;
    }

    public void Calculate(Alliance alliance, LogicKey key)
    {
        var d = key.Data;
        HashSet<Cell> prev;
        if (PrevContext is not null)
        {
            prev = PrevContext.AlliedCells.ToHashSet();
        }
        else
        {
            prev = new HashSet<Cell>();
        }

        PrevContext = Context;

        Context = new StrategicContext(alliance,
            prev,
            key.Data);

        var theaters = key.Data.GetAll<Theater>()
            .Where(t => t.Alliance.RefId == alliance.Id)
            .ToArray();
        var leaderAi = alliance.Leader.Get(key.Data).GetAi(key.Data);
        leaderAi.Status.Add("Doing strategic ai");
        if (theaters.Count() == 0)
        {
            leaderAi.Status.Add("Making theaters from scratch");

            MakeTheatersFromScratch(theaters, alliance, Context, key);
        }
        else
        {
            leaderAi.Status.Add("validating theaters");
            ValidateTheaters(theaters, alliance, Context, key);
        }

        leaderAi.Status.Add("validating frontlines");
        ValidateFrontlines(alliance, key);
        leaderAi.Status.Add("finished strategic ai");

        if (Context.Gained.Count > 0 || Context.Lost.Count > 0)
        {
            d.ClientPlayerData.Issues.Add(new StrategicContextIssue(leaderAi.Regime.Get(d),
                Context, d));
        }
    }

    private void MakeTheatersFromScratch(
        Theater[] theaters,
        Alliance alliance,
        StrategicContext context,
        LogicKey key)
    {
        var d = key.Data;

        foreach (var frontline in theaters.SelectMany(t => t.Frontlines.Entities(d)))
        {
            key.Remove(frontline);
        }

        foreach (var theater in theaters)
        {
            key.Remove(theater);
        }

        foreach (var union in context.Unions)
        {
            var theater = Theater.Create(alliance,
                union, key);
        }
    }

    private void ValidateTheaters(Theater[] theaters,
        Alliance alliance, StrategicContext context, LogicKey key)
    {
        var d = key.Data;

        var merge =
            context.Unions.ToDictionary(v => v,
                v => new List<Theater>());
        foreach (var theater in theaters)
        {
            var theaterCell = theater.Cells
                .Select(r => r.Get(d))
                .FirstOrDefault(context.AlliedCells.Contains);
            if (theaterCell is null)
            {
                //clean up
                key.Remove(theater);
                continue;
            }

            var mergeIntos = merge
                .Keys
                .Where(k => k.Contains(theaterCell));
            if (mergeIntos.Any() == false)
            {
                throw new Exception();
            }

            foreach (var mergeInto in mergeIntos)
            {
                merge[mergeInto].Add(theater);
            }
        }

        foreach (var (union, theatersToMerge) in merge)
        {
            if (theatersToMerge.Count == 0)
            {
                var theater = Theater.Create(alliance,
                    union.ToHashSet(),
                    key);
            }
            else
            {
                foreach (var theater in theatersToMerge)
                {
                    key.Remove(theater);
                }

                var newTheater = Theater.Create(alliance,
                    union.ToHashSet(),
                    key);
            }
        }
    }

    private void ValidateFrontlines(Alliance alliance, LogicKey key)
    {
        var oldFrontlines = key.Data.GetAll<Frontline>()
            .Where(fl => fl.Alliance.RefId == alliance.Id)
            .ToArray();
        FrontlineCache = oldFrontlines.ToDictionary(v => v.MakeRef(), v => v);
        foreach (var oldFrontline in oldFrontlines)
        {
            key.Remove(oldFrontline);
        }
        var theaters = key.Data.GetAll<Theater>()
            .Where(t => t.Alliance.RefId == alliance.Id).ToArray();

        MakeContextFrontGraph(alliance, key);
        ConstructNewFrontlinesWithGraph(alliance, key);
        FindEdgeMerges(alliance, key);
        FindFrontlineMerges(alliance, key);

        var newFrontlines = key.Data.GetAll<Frontline>()
            .Where(fl => fl.Alliance.RefId == alliance.Id).ToArray();
        foreach (var newFrontline in newFrontlines)
        {
            var theater =
                theaters.First(t => t.Cells.Contains(newFrontline.Faces.First().GetNative(key.Data).MakeRef()));
            var proc = new AddTheaterFrontlineProcedure(theater.MakeRef(),
                newFrontline.MakeRef());
            key.SendMessage(proc);
        }
    }


    private Dictionary<Frontline, List<Frontline>>
        MakeContextFrontGraph(
            Alliance alliance, LogicKey key)
    {
        var res = new Dictionary<Frontline, List<Frontline>>();

        handleRuns(Context.Stable, CellChangeStatus.Stable);
        handleRuns(Context.Gained, CellChangeStatus.Gained);
        handleRuns(Context.Lost, CellChangeStatus.Lost);

        return res;


        void handleRuns(HashSet<Cell> cells,
            CellChangeStatus status)
        {
            var fronts = FrontFinder.FindFrontsLeftToRight(
                cells,
                c => cells.Contains(c) == false
                     && c.Controller.Fulfilled(),
                key.Data);

            foreach (var front in fronts)
            {
                var fragments = new List<List<FrontFace>>();

                front.DoForRuns(
                    f => Context.GetCellChangeStatus(f.GetForeign(key.Data)),
                    fragments.Add,
                    (f, g) =>
                        f.GetLeftNeighborCellId(key.Data) == g.GetRightNeighborCellId(key.Data)
                );

                foreach (var fragment in fragments)
                {
                    var opposingStatus = Context.GetCellChangeStatus(fragment[0].GetForeign(key.Data));
                    if ((int)status >= (int)opposingStatus) continue;

                    var (start, end) = fragment.GetStartEndNexus(key.Data);

                    if (Context.Graph.HasEdge(start, end))
                    {
                        Context.Graph.GetEdge(start, end).Add(fragment);
                    }
                    else
                    {
                        Context.Graph.AddEdge(start, end, new HashSet<List<FrontFace>> { fragment });
                    }
                }
            }
        }

        
    }
    
    private void ConstructNewFrontlinesWithGraph(Alliance alliance, LogicKey key)
    {
        var validEdges = Context.Graph.Edges
            .SelectMany(fs => fs)
            .Where(f => ValidEdge(f, alliance, key.Data)).ToHashSet();
        var byLeft = new Dictionary<Vector3I, List<FrontFace>>();
        var byRight = new Dictionary<Vector3I, List<FrontFace>>();
        foreach (var faces in validEdges)
        {
            var (start, end) = faces.GetStartEndNexus(key.Data);
            byLeft.Add(start, faces);
            byRight.Add(end, faces);
        }

        while (validEdges.Count > 0)
        {
            var first = validEdges.First();
            var newFronts = new List<List<FrontFace>>{};
            remove(first);
            var left = first.GetStartEndNexus(key.Data).start;
            while (byRight.ContainsKey(left))
            {
                var nextLeft = byRight[left];
                newFronts.Add(nextLeft);
                remove(nextLeft);
                left = nextLeft.GetStartEndNexus(key.Data).start;
            }
            newFronts.Add(first);

            var right = first.GetStartEndNexus(key.Data).end;
            while (byLeft.ContainsKey(right))
            {
                var nextRight = byLeft[right];
                newFronts.Add(nextRight);
                remove(nextRight);
                right = nextRight.GetStartEndNexus(key.Data).end;
            }

            var newFrontFaces = new List<FrontFace>();
            for (var i = 0; i < newFronts.Count; i++)
            {
                
                var f = newFronts[i];
                for (var j = 0; j < f.Count; j++)
                {
                    newFrontFaces.Add(f[j]);
                }
            }

            var frontline = Frontline.Create(newFrontFaces, new HashSet<CellRef>(),
                alliance, key);
            foreach (var f in newFronts)
            {
                Context.ValidEdgesFrontlines[f] = frontline.MakeRef();
            }

            void remove(List<FrontFace> f)
            {
                validEdges.Remove(f);
                var (left, right) = f.GetStartEndNexus(key.Data);
                byLeft.Remove(left);
                byRight.Remove(right);
            }
        }

    }

    private bool ValidEdge(List<FrontFace> f, Alliance alliance, Data d)
    {
        var native = f[0].GetNative(d);
        var foreign = f[0].GetForeign(d);

        return native.FriendlyControlled(alliance, d)
               && foreign.RivalControlled(alliance, d);
    }
    private void FindEdgeMerges(Alliance alliance, LogicKey key)
    {
        // if (FrontlineCache is null || FrontlineCache.Count == 0) return;
        var edgeMergeMap = new Dictionary<List<FrontFace>, HashSet<List<FrontFace>>>();
        foreach (var graphEdge in Context.Graph.Edges)
        {
            foreach (var faces in graphEdge)
            {
                if (edgeMergeMap.ContainsKey(faces) == false)
                {
                    edgeMergeMap.Add(faces, new HashSet<List<FrontFace>>());
                }
                if (ValidEdge(faces, alliance, key.Data))
                {
                    edgeMergeMap[faces].Add(faces);
                }
                else if (graphEdge.FirstOrDefault(e => ValidEdge(e, alliance, key.Data))
                         is List<FrontFace> ve)
                {
                    edgeMergeMap[faces].Add(ve);
                }
                else
                {
                    var leftValid = findNextValid(faces, true);
                    var rightValid = findNextValid(faces, false);
                    if (leftValid is not null) edgeMergeMap[faces].Add(leftValid);
                    if (rightValid is not null) edgeMergeMap[faces].Add(rightValid);
                }
            }
        }

        Context.EdgeMergeMap.AddRange(edgeMergeMap);
        
        List<FrontFace> findNextValid(List<FrontFace> list, 
            bool goLeft)
        {
            var (left, right) = list.GetStartEndNexus(key.Data);
            var (nexus, otherNexus) = goLeft
                ? (left, right)
                : (right, left);
            var adjEdges = Context.Graph.GetNeighbors(nexus)
                .Where(n => n != otherNexus)
                .SelectMany(n => Context.Graph.GetEdge(nexus, n))
                .ToArray();
            if (adjEdges.FirstOrDefault(f => ValidEdge(f, alliance, key.Data)
                                    && Context.GetCellChangeStatus(f[0].GetNative(key.Data)) == CellChangeStatus.Gained)
                is List<FrontFace> res1)
            {
                return res1;
            }
            if (adjEdges.FirstOrDefault(f => ValidEdge(f, alliance, key.Data)
                                    && Context.GetCellChangeStatus(f[0].GetForeign(key.Data)) == CellChangeStatus.Lost)
                is List<FrontFace> res2)
            {
                return res2;
            }
            if (adjEdges.FirstOrDefault(f => ValidEdge(f, alliance, key.Data))
                is List<FrontFace> res3)
            {
                return res3;
            }

            if (adjEdges.Length == 1)
            {
                return findNextValid(adjEdges[0], goLeft);
            }

            return null;
        }
    }

    private void FindFrontlineMerges(Alliance alliance, LogicKey key)
    {
        var edgeHashes 
            = Context.Graph.Edges.SelectMany(v => v)
            .ToDictionary(l => l.ToHashSet(), l => l);
        FrontlineMerges = new Dictionary<ERef<Frontline>, HashSet<ERef<Frontline>>>();
        foreach (var (frontlineRef, frontline) in FrontlineCache)
        {
            var faces = frontline.Faces
                
                .ToHashSet();
            var mergeInto = new HashSet<ERef<Frontline>>();
            while (faces.Count > 0)
            {
                var first = faces.First();
                faces.Remove(first);
                var matchH = edgeHashes
                    .Keys
                    .FirstOrDefault(h => h.Contains(first));
                if (matchH is null)
                {
                    var issue = new CantFindFrontlineMergeIssue(
                        alliance,
                        first, frontline, Context, key.Data);
                    key.Data.ClientPlayerData.Issues.Add(issue);
                    continue;
                }

                var match = edgeHashes[matchH];
                faces.ExceptWith(matchH);
                if (Context.EdgeMergeMap.ContainsKey(match) == false)
                    continue;
                var validEdgesMapped
                    = Context.EdgeMergeMap[match];
                var validFrontlines = validEdgesMapped
                    .Select(e => Context.ValidEdgesFrontlines[e]);
                
                mergeInto.UnionWith(validFrontlines);
            }
            FrontlineMerges.Add(frontlineRef, mergeInto);
        }
    }

    // private Dictionary<Frontline, List<Frontline>> GetFrontlineMerges(
    //     Alliance alliance, LogicKey key)
    // {
    //     var newFrontFaces = Frontline.GetFacesFromCellsLToR(
    //         Context.AlliedCells, alliance, key.Data);
    //     var oldFrontlines = key.Data.GetAll<Frontline>()
    //         .Where(fl => fl.Alliance.RefId == alliance.Id).ToArray();
    //
    //     var newFrontlines = newFrontFaces
    //         .Select(fs => Frontline.Create(fs, 
    //             new HashSet<CellRef>(),
    //             alliance, key)).ToArray();
    //     
    //     HashSet<Cell> prev;
    //     if (PrevContext is not null)
    //     {
    //         prev = PrevContext.AlliedCells.ToHashSet();
    //     }
    //     else
    //     {
    //         prev = new HashSet<Cell>();
    //     }
    //
    //     var unionToNewMap = new Dictionary<HashSet<Cell>, 
    //         List<Frontline>>();
    //     foreach (var gainedUnion in Context.GainedUnions)
    //     {
    //         unionToNewMap.Add(gainedUnion, new List<Frontline>());
    //     }
    //     foreach (var lostUnion in Context.LostUnions)
    //     {
    //         unionToNewMap.Add(lostUnion, new List<Frontline>());
    //     }
    //     foreach (var stableUnion in Context.StableUnions)
    //     {
    //         unionToNewMap.Add(stableUnion, new List<Frontline>());
    //     }
    //     
    //     foreach (var newFrontline in newFrontlines)
    //     {
    //         foreach (var newFrontFace in newFrontline.Faces)
    //         {
    //             var native = newFrontFace.GetNative(key.Data);
    //             var foreign = newFrontFace.GetForeign(key.Data);
    //
    //             if (Context.Stable.Contains(native))
    //             {
    //                 var stableUnion = Context.StableUnions.First(u => u.Contains(native));
    //                 unionToNewMap[stableUnion].Add(newFrontline);
    //             }
    //             if (Context.Lost.Contains(foreign))
    //             {
    //                 var lostUnion = Context.LostUnions.First(u => u.Contains(foreign));
    //                 unionToNewMap[lostUnion].Add(newFrontline);
    //             }
    //             if (Context.Gained.Contains(native))
    //             {
    //                 var gainedUnion = Context.GainedUnions.First(u => u.Contains(native));
    //                 unionToNewMap[gainedUnion].Add(newFrontline);
    //             }
    //         }
    //     }
    //
    //     var oldToUnionMap = new Dictionary<Frontline, List<HashSet<Cell>>>();
    //     
    //     foreach (var oldFrontline in oldFrontlines)
    //     {
    //         oldToUnionMap.Add(oldFrontline, new List<HashSet<Cell>>());
    //         foreach (var oldFrontFace in oldFrontline.Faces)
    //         {
    //             var native = oldFrontFace.GetNative(key.Data);
    //             var foreign = oldFrontFace.GetForeign(key.Data);
    //
    //             if (Context.Stable.Contains(native))
    //             {
    //                 var stableUnion = Context.StableUnions.First(u => u.Contains(native));
    //                 oldToUnionMap[oldFrontline].Add(stableUnion);
    //             }
    //             if (Context.Lost.Contains(native))
    //             {
    //                 var lostUnion = Context.LostUnions.First(u => u.Contains(native));
    //                 oldToUnionMap[oldFrontline].Add(lostUnion);
    //             }
    //             if (Context.Gained.Contains(foreign))
    //             {
    //                 var gainedUnion = Context.GainedUnions.First(u => u.Contains(foreign));
    //                 oldToUnionMap[oldFrontline].Add(gainedUnion);
    //             }
    //         }
    //     }
    //     
    //     
    //     var mergeMap = new Dictionary<Frontline, List<Frontline>>();
    //     
    //     foreach (var (oldFrontline, unions) in oldToUnionMap)
    //     {
    //         mergeMap.Add(oldFrontline, new List<Frontline>());
    //         int iter = 0;
    //         foreach (var union in unions)
    //         {
    //             foreach (var newFrontline in unionToNewMap[union])
    //             {
    //                 mergeMap[oldFrontline].Add(newFrontline);
    //                 iter++;
    //             }
    //         }
    //
    //         if (iter == 0)
    //         {
    //             GD.Print("couldnt find merge frontline");
    //             var pos = oldFrontline.Faces.First().GetNative(key.Data).GetCenter();
    //
    //             var issue = new CustomIssue(pos,
    //                 $"{alliance.Leader.Get(key.Data).Name} couldnt find merge frontline",
    //                 key.Data.GetTick(), ("base", c =>
    //                 {
    //                     foreach (var cell in Context.Stable)
    //                     {
    //                         c.DrawCellRel(cell, pos, Colors.Blue, key.Data);
    //                     }
    //                     foreach (var cell in Context.Gained)
    //                     {
    //                         c.DrawCellRel(cell, pos, Colors.Green, key.Data);
    //                     }
    //                     foreach (var cell in Context.Lost)
    //                     {
    //                         c.DrawCellRel(cell, pos, Colors.Red, key.Data);
    //                     }
    //                     foreach (var union in unions)
    //                     {
    //                         foreach (var cell in union)
    //                         {
    //                             c.DrawCellRel(cell, pos, Colors.Orange, key.Data);
    //                         }
    //                     }
    //
    //                     foreach (var union in unions)
    //                     {
    //                         foreach (var newFrontline in unionToNewMap[union])
    //                         {
    //                             c.DrawFrontFaces(newFrontline.Faces, Colors.Red, 10f, pos, key.Data);
    //                         }
    //                     }
    //
    //                     c.DrawFrontFaces(oldFrontline.Faces, Colors.Yellow, 5f, pos, key.Data);
    //
    //                 }));
    //             key.Data.ClientPlayerData.Issues.Add(issue);
    //         }
    //     }
    //
    //     return mergeMap;
    // }
}