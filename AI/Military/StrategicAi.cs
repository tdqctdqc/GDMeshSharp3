
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using MessagePack;

public class StrategicAi
{
    public HashSet<CellRef> PrevOccupation { get; private set; }
    public Dictionary<ERef<Frontline>, List<ERef<Frontline>>> FrontlineMerges { get; private set; }
    public static StrategicAi Construct(Alliance a, Data d)
    {
        return new StrategicAi(new HashSet<CellRef>(),
            new Dictionary<ERef<Frontline>, List<ERef<Frontline>>>());
    }
    [SerializationConstructor] private StrategicAi(
        HashSet<CellRef> prevOccupation, 
        Dictionary<ERef<Frontline>, List<ERef<Frontline>>> frontlineMerges)
    {
        FrontlineMerges = frontlineMerges;
        PrevOccupation = prevOccupation;
    }

    public void Calculate(Alliance alliance, LogicKey key)
    {
        var d = key.Data;
        var context = new StrategicContext(alliance, key.Data);
        
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
        var theaters = key.Data.GetAll<Theater>()
            .Where(t => t.Alliance.RefId == alliance.Id).ToArray();
        
        var merge = GetFrontlineMerges(alliance, context, key);
        
        FrontlineMerges = merge.ToDictionary(kvp => kvp.Key.MakeRef(),
            kvp => kvp.Value.Select(v => v.MakeRef()).ToList());
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
        
        var gained = context.AlliedCells.Except(prev).ToHashSet();
        var gainedUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            gained, (c, d) => true, c => c.GetNeighbors(key.Data));
        
        var lost = prev.Except(context.AlliedCells).ToHashSet();
        var lostUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            lost, (c, d) => true, c => c.GetNeighbors(key.Data));

        var stable = context.AlliedCells.Intersect(prev).ToHashSet();
        var stableUnions = UnionFind.Find<Cell, HashSet<Cell>>(
            stable, (c, d) => true, c => c.GetNeighbors(key.Data));


        var unionToNewMap = new Dictionary<HashSet<Cell>, 
            List<Frontline>>();
        foreach (var gainedUnion in gainedUnions)
        {
            unionToNewMap.Add(gainedUnion, new List<Frontline>());
        }
        foreach (var lostUnion in lostUnions)
        {
            unionToNewMap.Add(lostUnion, new List<Frontline>());
        }
        foreach (var stableUnion in stableUnions)
        {
            unionToNewMap.Add(stableUnion, new List<Frontline>());
        }
        
        foreach (var newFrontline in newFrontlines)
        {
            foreach (var newFrontFace in newFrontline.Faces)
            {
                var native = newFrontFace.GetNative(key.Data);
                var foreign = newFrontFace.GetForeign(key.Data);

                if (stable.Contains(native))
                {
                    var stableUnion = stableUnions.First(u => u.Contains(native));
                    unionToNewMap[stableUnion].Add(newFrontline);
                }
                if (lost.Contains(foreign))
                {
                    var lostUnion = lostUnions.First(u => u.Contains(foreign));
                    unionToNewMap[lostUnion].Add(newFrontline);
                }
                if (gained.Contains(native))
                {
                    var gainedUnion = gainedUnions.First(u => u.Contains(native));
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

                if (stable.Contains(native))
                {
                    var stableUnion = stableUnions.First(u => u.Contains(native));
                    oldToUnionMap[oldFrontline].Add(stableUnion);
                }
                if (lost.Contains(native))
                {
                    var lostUnion = lostUnions.First(u => u.Contains(native));
                    oldToUnionMap[oldFrontline].Add(lostUnion);
                }
                if (gained.Contains(foreign))
                {
                    var gainedUnion = gainedUnions.First(u => u.Contains(foreign));
                    oldToUnionMap[oldFrontline].Add(gainedUnion);
                }
            }
        }
        
        
        var mergeMap = new Dictionary<Frontline, List<Frontline>>();
        
        foreach (var (oldFrontline, unions) in oldToUnionMap)
        {
            mergeMap.Add(oldFrontline, new List<Frontline>());
            foreach (var union in unions)
            {
                foreach (var newFrontline in unionToNewMap[union])
                {
                    mergeMap[oldFrontline].Add(newFrontline);
                }
            }
        }

        return mergeMap;
    }
}