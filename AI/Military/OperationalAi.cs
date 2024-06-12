
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
            var rival = frontline.Faces
                .Select(f => f.GetForeign(_data))
                .Distinct()
                .Where(f => f.Controller.Get(_data)
                        .GetAlliance(_data).IsRivals(Alliance, _data))
                .ToHashSet();
            
            if (rival.Count == 0)
            {
                continue;
            }
            
            var enemyPowerPoints = rival
                .Sum(c => _data.Context.PowerPoints[c]);
            var friendly = frontline.Faces
                .Select(f => f.GetNative(_data)).Distinct().ToArray();
            var friendlyPowerPoints = friendly
                .Sum(c => _data.Context.PowerPoints[c]);;

            if (
                // true || 
                friendlyPowerPoints > 1.5f * enemyPowerPoints
                )
            {
                GeneralAdvance(frontline, rival);
            }
        }
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