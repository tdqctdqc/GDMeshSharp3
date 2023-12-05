
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ForceBalance 
{
    public Dictionary<Regime, float> ByRegime { get; private set; }
    public Dictionary<Alliance, float> ByAlliance { get; private set; }
    public HashSet<Unit> Units { get; private set; }

    public ForceBalance()
    {
        Units = new HashSet<Unit>();
        ByRegime = new Dictionary<Regime, float>();
        ByAlliance = new Dictionary<Alliance, float>();
    }

    public void ClearBalance()
    {
        Units.Clear();
        ByAlliance.Clear();
        ByRegime.Clear();
    }
    public void Add(Unit unit, Data data)
    {
        Units.Add(unit);
        var pp = unit.GetPowerPoints(data);
        var regime = unit.Regime.Entity(data);
        var alliance = regime.GetAlliance(data);
        ByAlliance.AddOrSum(alliance, pp);
        ByRegime.AddOrSum(regime, pp);
    }
    public void Add(Regime regime, float amount, Data d)
    {
        var alliance = regime.GetAlliance(d);
        ByAlliance.AddOrSum(alliance, amount);
        ByRegime.AddOrSum(regime, amount);
    }

    public Alliance GetMostPowerfulAlliance()
    {
        if (ByAlliance.Count == 0) return null;
        return ByAlliance.MaxBy(kvp => kvp.Value)
            .Key;
    }
    public IEnumerable<Regime> GetControllingRegimes()
    {
        return ByRegime.Keys.Where(a => ByRegime[a] > 0f);
    }
    public IEnumerable<Alliance> GetControllingAlliances()
    {
        return ByAlliance.Keys.Where(a => ByAlliance[a] > 0f);
    }

    public bool IsAllianceControlling(Alliance a)
    {
        return ByAlliance.ContainsKey(a) && ByAlliance[a] > 0f;
    }

    public float GetHostilePowerPoints(Alliance a, Data d)
    {
        return ByAlliance.Where(kvp => a.Rivals.Contains(a))
            .Sum(kvp => kvp.Value);
    }
    public float GetHostilePowerPointsOfNeighbors(Waypoint wp,
        Alliance a, Data d)
    {
        var res = 0f;
        foreach (var n in wp.TacNeighbors(d))
        {
            if (n.IsDirectlyThreatened(a, d))
            {
                res += d.Context.WaypointForceBalances[n].GetHostilePowerPoints(a, d);
            }
        }
        return res;
    }
}