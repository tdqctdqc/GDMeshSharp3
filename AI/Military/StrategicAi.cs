
using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;

public class StrategicAi
{ 
    public ERefSet<Theater> Theaters { get; private set; }
    public Dictionary<ERef<Frontline>, FrontlineAi> FrontlineAis { get; private set; }

    public static StrategicAi Construct(Alliance a, Data d)
    {
        return new StrategicAi(ERefSet<Theater>.Construct(new ERef<Theater>[]{}),
            new Dictionary<ERef<Frontline>, FrontlineAi>());
    }
    [SerializationConstructor] private StrategicAi(
        ERefSet<Theater> theaters, 
        Dictionary<ERef<Frontline>, FrontlineAi> frontlineAis)
    {
        Theaters = theaters;
        FrontlineAis = frontlineAis;
    }

    public void Calculate(Alliance alliance, LogicKey key)
    {
        var d = key.Data;
        
        MakeTheatersFromScratch(alliance, key);
        // if (Theaters.Count() == 0)
        // {
        //     
        // }
        // else
        // {
        //     ValidateTheaters(alliance, key);
        // }
        foreach (var theater in Theaters.Entities(d))
        {
            CalculateTheater(alliance, theater, key);
        }
    }

    private void MakeTheatersFromScratch(Alliance alliance, LogicKey key)
    {
        var d = key.Data;
        var cells = d.Planet.MapAux
            .CellHolder.Cells.Values
            .Where(c => alliance.Members.Contains(c.Controller))
            .ToArray();
        var unions = UnionFind.Find<Cell, HashSet<Cell>>(cells,
            (p, q) => true,
            p => p.GetNeighbors(d));
        
        d.RemoveEntities(Theaters.Entities(d).SelectMany(t => t.Frontlines.Entities(d).Select(fl => fl.Id)).ToArray(),
            key);
        d.RemoveEntities(Theaters.Refs.Select(r => r.RefId).ToArray(), key);
        FrontlineAis.Clear();
        Theaters = ERefSet<Theater>.Construct(new HashSet<int>());
        foreach (var union in unions)
        {
            var theater = Theater.Create(alliance, 
                union, key);
            Theaters.Add(theater, key);
            theater.MakeFrontlinesFromScratch(key);
            foreach (var frontline in theater.Frontlines.Entities(d))
            {
                FrontlineAis.Add(frontline.MakeRef(), FrontlineAi.Construct(frontline));
            }
        }
    }

    private void ValidateTheaters(Alliance alliance, LogicKey key)
    {
        var d = key.Data;
        var cells = d.Planet.MapAux
            .CellHolder.Cells.Values
            .Where(c => alliance.Members.Contains(c.Controller))
            .ToArray();
        var unions = UnionFind.Find<Cell, HashSet<Cell>>(cells,
            (p, q) => true,
            p => p.GetNeighbors(d));
        var merge = unions.ToDictionary(v => v,
            v => new List<Theater>());
        foreach (var theater in Theaters.Entities(d).ToArray())
        {
            var theaterCell = theater.Cells
                .Select(r => r.Get(d))
                .Where(c => c.FriendlyControlled(alliance, d))
                .FirstOrDefault();
            if (theaterCell is null)
            {
                //clean up
                Theaters.Remove(theater.MakeRef(), key);
                key.Data.RemoveEntity(theater.Id, key);
                continue;
            }
            var mergeIntos = merge
                .Keys
                .Where(k => k.Contains(theaterCell));
            foreach (var mergeInto in mergeIntos)
            {
                merge[mergeInto].Add(theater);
            }
        }
        


    }
    
    
    private void CalculateTheater(Alliance alliance, Theater theater, 
        LogicKey key)
    {
        var d = key.Data;
        foreach (var frontline in theater.Frontlines.Entities(d))
        {
            var length = frontline.Faces.Count;
            var frontlineAi = FrontlineAis[frontline.MakeRef()];
            frontlineAi.MakeReport(d);
            var report = frontlineAi.Report;
            var hostilePp = report.HostileOnFront.Any()
                ? report.HostileOnFront.Sum(
                    c => d.Context.PowerPoints[c]) 
                : 0f;
            hostilePp *= .5f;
            var rivalPp = report.RivalOnFront.Sum(
                c => d.Context.PowerPoints[c]) * 5f;
            var opposing = hostilePp + rivalPp;
            var oppNeed = opposing * MilUtil.DesiredOpposingPpRatio;
            var lengthNeed = length * MilUtil.PowerPointsPerCellFaceToCover;
            
            frontlineAi.AddDefendWeightAlongWholeLine(oppNeed + lengthNeed, d);
        }
        var reports = FrontlineAis.Values.Select(v => v.Report);
        var allHostile = reports
            .SelectMany(kvp => kvp.HostileOnFront).ToHashSet();
        
        var friendlyPower = reports.Sum(r => r.FriendlyPower);
        var enemyPower = reports.Sum(r => r.EnemyPower);
        var availablePowerForOffense = friendlyPower - enemyPower * .8f;
        
        if (availablePowerForOffense <= 0f) return;
        
        foreach (var (frontlineAi, pocket) in FrontlineAis.Values
                     .SelectMany(kvp => kvp.Report.Pockets
                         .Select(pocket => (kvp, pocket)))
                     .OrderBy(v => v.pocket.Count))
        {
            
            var pocketPower = pocket.Sum(c => d.Context.PowerPoints[c]);
            var commit = 1.5f * pocketPower;
            availablePowerForOffense -= commit;
            frontlineAi.AddAttackWeight(commit, pocket, key);
            allHostile.ExceptWith(pocket);
            if (availablePowerForOffense <= 0f) break;
        }
        //find 'necks' 
        
        foreach (var hostile in allHostile.OrderBy(getAtkScore))
        {
            if (availablePowerForOffense <= 0f) break;
            var commit = d.Context.PowerPoints[hostile] * 1.5f;
            availablePowerForOffense -= commit;
            foreach (var frontlineAi in theater.Frontlines.Entities(d)
                         .Select(fl => FrontlineAis[fl.MakeRef()])
                         .Where(flAi => flAi.Report.RivalOnFront.Contains(hostile)))
            {
                frontlineAi.AddAttackWeight(commit, hostile, key);
            }
        }
        

        float getAtkScore(Cell hCell)
        {
            var pp = d.Context.PowerPoints[hCell];
            var adj = hCell.GetNeighbors(d)
                .Count(c => c.Controller.RefId == alliance.Id);
            return pp / adj;
        }
    }
    
    
}