
using System.Collections.Generic;
using System.Linq;
using Godot;

public class OperationalAi
{
    public Alliance Alliance { get; private set; }
    private Data _data;
    public OperationalAi(Data data, Alliance alliance)
    {
        _data = data;
        Alliance = alliance;
    }

    public void Calculate(AllianceMilitaryAi ai)
    {
        foreach (var theater in ai.Strategic.Theaters)
        {
            CalculateTheater(theater);
        }
    }

    private void CalculateTheater(Theater theater)
    {
        foreach (var frontline in theater.Frontlines)
        {
            var hostiles = frontline.Faces
                .Select(f => f.GetForeign(_data))
                .Distinct()
                .Where(f => f.Controller.Get(_data)
                        .GetAlliance(_data).IsAtWar(Alliance, _data))
                .ToHashSet();
            var friendlies = frontline.Faces.Select(f => f.GetNative(_data))
                .ToHashSet();
            
            if (hostiles.Count == 0)
            {
                continue;
            }
            
            
            var enemyPowerPoints = hostiles
                .Sum(c => _data.Context.PowerPoints[c]);
            var friendly = frontline.Faces
                .Select(f => f.GetNative(_data)).Distinct().ToArray();
            var friendlyPowerPoints = friendly
                .Sum(c => _data.Context.PowerPoints[c]);;
            
            // if (
            //     // true || 
            //     friendlyPowerPoints > 1.5f * enemyPowerPoints
            //     )
            // {
            //     GeneralAdvance(frontline, hostile);
            // }
            
            var availablePowerForOffense = friendlyPowerPoints - enemyPowerPoints * .8f;
            var toHandle = hostiles.ToHashSet();
            var hostileUnions = FindHostileUnions(hostiles);
            var maxPocketSize = 10;
            var pockets = hostileUnions.Where(u => u.Count <= maxPocketSize);
            foreach (var pocket in pockets.OrderBy(p => p.Count))
            {
                if (availablePowerForOffense <= 0f) return;
                var pocketPower = pocket.Sum(c => _data.Context.PowerPoints[c]);
                availablePowerForOffense -= pocketPower;
                frontline.AdvanceInto.UnionWith(pocket);
                toHandle.ExceptWith(pocket);
            }
            
            if (availablePowerForOffense <= 0f) return;

            //find 'necks' 

            float getAtkScore(Cell hCell)
            {
                var pp = _data.Context.PowerPoints[hCell];
                var adj = hCell.GetNeighbors(_data)
                    .Count(friendlies.Contains);
                return pp / adj;
            }
            
            
            foreach (var cell in toHandle.OrderBy(getAtkScore))
            {
                if (availablePowerForOffense <= 0f) return;
                var pp = _data.Context.PowerPoints[cell];
                availablePowerForOffense -= pp;
                frontline.AdvanceInto.Add(cell);
            }
        }
    }
    
    private List<HashSet<Cell>> FindHostileUnions(HashSet<Cell> hostile)
    {
        var seeds = hostile.ToHashSet();
        var unions = new List<HashSet<Cell>>();
        while (seeds.Any())
        {
            var seed = seeds.First();
            var alliance = seed.Controller.Get(_data).GetAlliance(_data);
            seeds.Remove(seed);
            var flood = FloodFill<Cell>
                .GetFloodFill(seed,
                c => c.Controller.Fulfilled()
                    && c.Controller.Get(_data)
                        .GetAlliance(_data) == alliance,
                c => c.GetNeighbors(_data));
            seeds.ExceptWith(flood);
            unions.Add(flood);
        }

        return unions;
    }

    private void GeneralAdvance(Frontline f, 
        HashSet<Cell> enemy)
    {
        HashSet<Cell> advanceInto = enemy.ToHashSet();
        for (int i = 0; i < 1; i++)
        {
            advanceInto = advanceInto.Union(advanceInto.SelectMany(r => 
                    r.GetNeighbors(_data)
                        .Where(f => 
                            f.Controller.Fulfilled()
                            && advanceInto.Contains(f) == false
                            && f.Controller.Get(_data).GetAlliance(_data).IsAtWar(Alliance, _data))))
                .ToHashSet();
        }
        f.AdvanceInto.Clear();
        f.AdvanceInto.UnionWith(advanceInto);
    }
}