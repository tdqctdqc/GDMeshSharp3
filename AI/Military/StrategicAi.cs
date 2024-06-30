
using System.Collections.Generic;
using System.Linq;

public class StrategicAi
{
    public Alliance Alliance { get; private set; }
    private Data _data;
    public HashSet<Theater> Theaters { get; private set; }

    public StrategicAi(Data data, Alliance alliance)
    {
        _data = data;
        Alliance = alliance;
        
    }
    public void Calculate(Data d)
    {
        MakeTheaters();
        foreach (var theater in Theaters)
        {
            CalculateTheater(theater, d);
        }
    }

    private void MakeTheaters()
    {
        var alliance = Alliance;
        var cells = _data.Planet.MapAux
            .CellHolder.Cells.Values
            .Where(c => alliance.Members.Contains(c.Controller))
            .ToArray();
        var unions = UnionFind.Find(cells,
            (p, q) => true,
            p => p.GetNeighbors(_data));
        Theaters = new HashSet<Theater>();
        foreach (var union in unions)
        {
            var theater = Theater.Construct(Alliance, union.ToHashSet(), _data);
            Theaters.Add(theater);
        }
        
    }
    
    private void CalculateTheater(Theater theater, Data d)
    {
        //todo alter weights for rival not at war
        
        
        
        var reports = theater.Frontlines.ToDictionary(fl => fl,
            fl => new FrontlineTacticalReport(fl, d));
        
        
        foreach (var frontline in theater.Frontlines)
        {
            var length = frontline.Faces.Count;
            var report = reports[frontline];
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
            
            frontline.AddDefendWeightAlongWholeLine(oppNeed + lengthNeed, d);
        }
        
        
        
        
        var allHostile = reports
            .SelectMany(kvp => kvp.Value.HostileOnFront).ToHashSet();
        
        var friendlyPower = reports.Values.Sum(r => r.FriendlyPower);
        var enemyPower = reports.Values.Sum(r => r.EnemyPower);
        var availablePowerForOffense = friendlyPower - enemyPower * .8f;
        
        if (availablePowerForOffense <= 0f) return;
        
        foreach (var (frontline, pocket) in reports
                     .SelectMany(kvp => kvp.Value.Pockets
                         .Select(pocket => (kvp.Key, pocket)))
                     .OrderBy(v => v.pocket.Count))
        {
            
            var pocketPower = pocket.Sum(c => d.Context.PowerPoints[c]);
            var commit = 1.5f * pocketPower;
            availablePowerForOffense -= commit;
            frontline.AddAttackWeight(commit, pocket);
            allHostile.ExceptWith(pocket);
            if (availablePowerForOffense <= 0f) break;
        }
        //find 'necks' 
        
        foreach (var hostile in allHostile.OrderBy(getAtkScore))
        {
            if (availablePowerForOffense <= 0f) break;
            var commit = d.Context.PowerPoints[hostile] * 1.5f;
            availablePowerForOffense -= commit;
            foreach (var frontline in theater.Frontlines.Where(fl => reports[fl].RivalOnFront.Contains(hostile)))
            {
                frontline.AddAttackWeight(commit, hostile);
            }
        }
        

        float getAtkScore(Cell hCell)
        {
            var pp = d.Context.PowerPoints[hCell];
            var adj = hCell.GetNeighbors(d)
                .Count(c => c.Controller.RefId == Alliance.Id);
            return pp / adj;
        }
    }
    
    
}