
using System.Collections.Generic;
using System.Linq;

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
            rival = rival.Union(rival.SelectMany(r => 
                r.GetNeighbors(_data)
                    .Where(f => f.Controller.Fulfilled()
                        && f.Controller.Get(_data).GetAlliance(_data).IsRivals(Alliance, _data))))
                .ToHashSet();
            if (rival.Count == 0)
            {
                continue;
            }

            var enemyPowerPoints = rival
                .Where(c => c.GetUnits(_data) is not null)
                .SelectMany(c => c.GetUnits(_data))
                .Where(v => v is not null)
                .Sum(u => u.GetPowerPoints(_data));
            var friendly = frontline.Faces
                .Select(f => f.GetNative(_data)).Distinct().ToArray();
            var friendlyPowerPoints = friendly
                .Where(c => c.GetUnits(_data) is not null)
                .SelectMany(c => c.GetUnits(_data).Where(u => Alliance.Members.RefIds.Contains(u.Regime.RefId)))
                .Sum(u => u.GetPowerPoints(_data));

            if (
                true ||
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
        f.SetAdvanceInto(enemy, _data);
    }
}