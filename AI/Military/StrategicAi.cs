
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class StrategicAi
{
    public HashSet<CellRef> PrevOccupation { get; private set; }
    public Dictionary<ERef<Frontline>, HashSet<ERef<Frontline>>> FrontlineMerges { get; private set; }
    public Dictionary<ERef<Frontline>, Frontline> FrontlineCache { get; private set; }
    public static StrategicAi Construct(Alliance a, Data d)
    {
        return new StrategicAi(new HashSet<CellRef>(),
            new Dictionary<ERef<Frontline>, HashSet<ERef<Frontline>>>(),
            new Dictionary<ERef<Frontline>, Frontline>());
    }
    [SerializationConstructor] private StrategicAi(
        HashSet<CellRef> prevOccupation, 
        Dictionary<ERef<Frontline>, HashSet<ERef<Frontline>>> frontlineMerges, Dictionary<ERef<Frontline>, Frontline> frontlineCache)
    {
        FrontlineMerges = frontlineMerges;
        FrontlineCache = frontlineCache;
        PrevOccupation = prevOccupation;
    }

    public void Calculate(Alliance alliance, LogicKey key)
    {
        var d = key.Data;
        var context = new StrategicContext(alliance, 
            PrevOccupation.Select(p => p.Get(key.Data)).ToHashSet(),
            key.Data);
        
        var theaters = key.Data.GetAll<Theater>()
            .Where(t => t.Alliance.RefId == alliance.Id)
            .ToArray();
        var leaderAi = alliance.Leader.Get(key.Data).GetAi(key.Data);
        leaderAi.Status.Add("Doing strategic ai");
        if (theaters.Count() == 0)
        {
            leaderAi.Status.Add("Making theaters from scratch");

            MakeTheatersFromScratch(theaters, alliance, context, key);
        }
        else
        {
            leaderAi.Status.Add("validating theaters");
            ValidateTheaters(theaters, alliance, context, key);
        }
        leaderAi.Status.Add("validating frontlines");
        ValidateFrontlines(alliance, context, key);
        leaderAi.Status.Add("finished strategic ai");

        PrevOccupation = context.AlliedCells.Select(c => c.MakeRef()).ToHashSet();
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

    private void ValidateFrontlines(Alliance alliance,
        StrategicContext context, LogicKey key)
    {
        var oldFrontlines = key.Data.GetAll<Frontline>()
            .Where(fl => fl.Alliance.RefId == alliance.Id)
            .ToArray();
        FrontlineCache = oldFrontlines.ToDictionary(v => v.MakeRef(), v => v);
        var theaters = key.Data.GetAll<Theater>()
            .Where(t => t.Alliance.RefId == alliance.Id).ToArray();
        
        var merge = GetFrontlineMerges(alliance, context, key);
        
        FrontlineMerges = merge.ToDictionary(kvp => kvp.Key.MakeRef(),
            kvp => kvp.Value.Select(v => v.MakeRef()).ToHashSet());
        foreach (var oldFrontline in oldFrontlines)
        {
            key.Remove(oldFrontline);
        }

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

    // private Dictionary<Frontline, List<Frontline>> GetFrontlineMergesNew(
    //     Alliance alliance, StrategicContext context, LogicKey key)
    // {
    //     
    // }
    private Dictionary<Frontline, List<Frontline>> GetFrontlineMerges(
        Alliance alliance, StrategicContext context, LogicKey key)
    {
        var newFrontFaces = Frontline.GetFacesFromCells(
            context.AlliedCells, alliance, key.Data);
        var oldFrontlines = key.Data.GetAll<Frontline>()
            .Where(fl => fl.Alliance.RefId == alliance.Id).ToArray();

        var newFrontlines = newFrontFaces
            .Select(fs => Frontline.Create(fs, new HashSet<CellRef>(),
                alliance, key)).ToArray();
        
        
        var prev = PrevOccupation.Select(p => p.Get(key.Data)).ToHashSet();
        
        

        var unionToNewMap = new Dictionary<HashSet<Cell>, 
            List<Frontline>>();
        foreach (var gainedUnion in context.GainedUnions)
        {
            unionToNewMap.Add(gainedUnion, new List<Frontline>());
        }
        foreach (var lostUnion in context.LostUnions)
        {
            unionToNewMap.Add(lostUnion, new List<Frontline>());
        }
        foreach (var stableUnion in context.StableUnions)
        {
            unionToNewMap.Add(stableUnion, new List<Frontline>());
        }
        
        foreach (var newFrontline in newFrontlines)
        {
            foreach (var newFrontFace in newFrontline.Faces)
            {
                var native = newFrontFace.GetNative(key.Data);
                var foreign = newFrontFace.GetForeign(key.Data);

                if (context.Stable.Contains(native))
                {
                    var stableUnion = context.StableUnions.First(u => u.Contains(native));
                    unionToNewMap[stableUnion].Add(newFrontline);
                }
                if (context.Lost.Contains(foreign))
                {
                    var lostUnion = context.LostUnions.First(u => u.Contains(foreign));
                    unionToNewMap[lostUnion].Add(newFrontline);
                }
                if (context.Gained.Contains(native))
                {
                    var gainedUnion = context.GainedUnions.First(u => u.Contains(native));
                    unionToNewMap[gainedUnion].Add(newFrontline);
                }
            }
        }

        var oldToUnionMap = new Dictionary<Frontline, List<HashSet<Cell>>>();
        
        foreach (var oldFrontline in oldFrontlines)
        {
            oldToUnionMap.Add(oldFrontline, new List<HashSet<Cell>>());
            foreach (var oldFrontFace in oldFrontline.Faces)
            {
                var native = oldFrontFace.GetNative(key.Data);
                var foreign = oldFrontFace.GetForeign(key.Data);

                if (context.Stable.Contains(native))
                {
                    var stableUnion = context.StableUnions.First(u => u.Contains(native));
                    oldToUnionMap[oldFrontline].Add(stableUnion);
                }
                if (context.Lost.Contains(native))
                {
                    var lostUnion = context.LostUnions.First(u => u.Contains(native));
                    oldToUnionMap[oldFrontline].Add(lostUnion);
                }
                if (context.Gained.Contains(foreign))
                {
                    var gainedUnion = context.GainedUnions.First(u => u.Contains(foreign));
                    oldToUnionMap[oldFrontline].Add(gainedUnion);
                }
            }
        }
        
        
        var mergeMap = new Dictionary<Frontline, List<Frontline>>();
        
        foreach (var (oldFrontline, unions) in oldToUnionMap)
        {
            mergeMap.Add(oldFrontline, new List<Frontline>());
            int iter = 0;
            foreach (var union in unions)
            {
                foreach (var newFrontline in unionToNewMap[union])
                {
                    mergeMap[oldFrontline].Add(newFrontline);
                    iter++;
                }
            }

            if (iter == 0)
            {
                GD.Print("couldnt find merge frontline");
                var pos = oldFrontline.Faces.First().GetNative(key.Data).GetCenter();

                var issue = new CustomIssue(pos,
                    $"{alliance.Leader.Get(key.Data).Name} couldnt find merge frontline",
                    c =>
                    {
                        // foreach (var cell in stable)
                        // {
                        //     c.DrawCellRel(cell, pos, Colors.Blue, key.Data);
                        // }
                        // foreach (var cell in gained)
                        // {
                        //     c.DrawCellRel(cell, pos, Colors.Green, key.Data);
                        // }
                        // foreach (var cell in lost)
                        // {
                        //     c.DrawCellRel(cell, pos, Colors.Red, key.Data);
                        // }
                        foreach (var union in unions)
                        {
                            foreach (var cell in union)
                            {
                                c.DrawCellRel(cell, pos, Colors.Orange, key.Data);
                            }
                        }
                        foreach (var union in unions)
                        {
                            foreach (var newFrontline in unionToNewMap[union])
                            {
                                c.DrawFrontFaces(newFrontline.Faces, Colors.Red, 10f, pos, key.Data);
                            }
                        }
                        c.DrawFrontFaces(oldFrontline.Faces, Colors.Yellow, 5f, pos, key.Data);
                        
                    });
                key.Data.ClientPlayerData.Issues.Add(issue);
            }
        }

        return mergeMap;
    }
}