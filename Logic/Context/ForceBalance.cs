
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ForceBalance : Dictionary<Alliance, float>
{
    public static float PowerPointsForPolyOwnership = 10f;
    public void Add(Unit unit, Data data)
    {
        var alliance = unit.Regime.Entity(data).GetAlliance(data);
        this.AddOrSum(alliance, unit.GetPowerPoints(data));
    }
    public void Add(Alliance alliance, float amount)
    {
        this.AddOrSum(alliance, amount);
    }
    public IEnumerable<Alliance> GetControllingAlliances()
    {
        return this.Keys.Where(a => this[a] > 0f);
    }
}