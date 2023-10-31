
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ForceBalance : Dictionary<Alliance, float>
{
    public static float PowerRatioToCountAsControlled = .6f;
    public static float PowerPointsForPolyOwnership = 10f;
    private static float RatioToHaveForceSuperiority = 1.2f;
    private static float RatioToHaveForceSupremacy = 3f;
    public void Add(Unit unit, Data data)
    {
        var alliance = unit.Regime.Entity(data).GetAlliance(data);
        this.AddOrSum(alliance, unit.GetPowerPoints(data));
    }
    public void Add(Alliance alliance, float amount)
    {
        this.AddOrSum(alliance, amount);
    }
    public void DiffuseInto(Unit unit, Data data)
    {
        var alliance = unit.Regime.Entity(data).GetAlliance(data);
        this.AddOrSum(alliance, unit.GetPowerPoints(data) / 3f);
    }
    public IEnumerable<Alliance> GetControllingAlliances()
    {
        if (Values.Count() == 0) return Enumerable.Empty<Alliance>();
        var topPower = Values.Max();
        var minPowerToControl = topPower * PowerRatioToCountAsControlled;
        return this
            .Where(kvp => kvp.Value >= minPowerToControl)
            .Select(kvp => kvp.Key);
    }
    public Alliance GetAllianceWithForceSuperiority(Data data)
    {
        if (Count == 0) return null;
        if (Count == 1) return this.First().Key;
        var ordered = this.OrderBy(kvp => kvp.Value);
        var first = ordered.ElementAt(0);
        var second = ordered.ElementAt(1);
        if (first.Value <= second.Value * RatioToHaveForceSuperiority) return null;
        return first.Key;
    }
    
    public Alliance GetAllianceWithForceSupremacy(Data data)
    {
        if (Count == 0) return null;
        if (Count == 1) return this.First().Key;
        var ordered = this.OrderBy(kvp => kvp.Value);
        var first = ordered.ElementAt(0);
        var second = ordered.ElementAt(1);
        if (first.Value <= second.Value * RatioToHaveForceSupremacy) return null;
        return first.Key;
    }
}