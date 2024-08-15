
using System;
using System.Collections.Generic;
using System.Linq;

public class FrontlineTacticalReport
{
    public List<HashSet<Cell>> Pockets { get; private set; }
    public HashSet<Cell> RivalOnFront { get; private set; }
    public HashSet<Cell> HostileOnFront { get; private set; }
    public float EnemyPower { get; private set; }
    public float FriendlyPower { get; private set; }
    public FrontlineTacticalReport(Frontline frontline, Data data)
    {
        RivalOnFront = frontline.GetRivalOpposingCells(data);
        
        if (RivalOnFront.Count == 0)
        {
            throw new Exception();
        }

        HostileOnFront = RivalOnFront.Where(c => c.Controller.Get(data)
            .IsAtWar(frontline.Regime.Get(data), data)).ToHashSet();
        var friendlies = frontline.Faces.Select(f => f.GetNative(data))
            .ToHashSet();
        EnemyPower = RivalOnFront
            .Sum(c =>
            {
                var mult = HostileOnFront.Contains(c)
                    ? 1f
                    : Frontline.DefMultForNotAtWarCell;
                return mult * data.Context.PowerPoints[c];
            });
        var friendly = frontline.Faces
            .Select(f => f.GetNative(data)).Distinct().ToArray();
        FriendlyPower = friendly
            .Sum(c => data.Context.PowerPoints[c]);
        
        var hostileUnions = FindHostileUnions(HostileOnFront, data);
        var maxPocketSize = 10;
        Pockets = hostileUnions.Where(u => u.Count <= maxPocketSize)
            .ToList();
    }
    
    
    
    private List<HashSet<Cell>> FindHostileUnions(HashSet<Cell> hostile, Data data)
    {
        var seeds = hostile.ToHashSet();
        var unions = new List<HashSet<Cell>>();
        while (seeds.Any())
        {
            var seed = seeds.First();
            var regime = seed.Controller.Get(data);
            seeds.Remove(seed);
            var flood = FloodFill<Cell>
                .GetFloodFill(seed,
                    c => c.Controller.Fulfilled()
                         && c.Controller.RefId == regime.Id,
                    c => c.GetNeighbors(data));
            seeds.ExceptWith(flood);
            unions.Add(flood);
        }

        return unions;
    }
}