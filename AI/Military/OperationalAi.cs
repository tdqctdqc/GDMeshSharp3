
using System.Collections.Generic;
using System.Linq;

public class OperationalAi
{
    public Regime Regime { get; private set; }
    private Data _data;
    public OperationalAi(Data data, Regime regime)
    {
        _data = data;
        Regime = regime;
    }

    public void Calculate(RegimeMilitaryAi ai)
    {
        foreach (var theater in ai.Strategic.Theaters)
        {
            CalculateTheater(theater);
        }
    }

    private void CalculateTheater(Theater theater)
    {
        var alliance = Regime.GetAlliance(_data);
        foreach (var frontline in theater.Frontlines)
        {
            var rival = frontline.Faces
                .Select(f => f.GetForeign(_data))
                .Distinct()
                .Where(f => f.Controller.Entity(_data)
                        .GetAlliance(_data).IsRivals(alliance, _data))
                .ToHashSet();
            rival = rival.Union(rival.SelectMany(r => 
                r.GetNeighbors(_data)
                    .Where(f => f.Controller.Fulfilled()
                        && f.Controller.Entity(_data).GetAlliance(_data).IsRivals(alliance, _data))))
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
                .SelectMany(c => c.GetUnits(_data).Where(u => u.Regime.RefId == Regime.Id))
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