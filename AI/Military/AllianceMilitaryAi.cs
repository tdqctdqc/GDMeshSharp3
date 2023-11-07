
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    private Alliance _alliance;
    public Dictionary<Regime, HashSet<Waypoint>> AreasOfResponsibility { get; private set; }
    public AllianceMilitaryAi(Alliance alliance)
    {
        _alliance = alliance;
        AreasOfResponsibility = new Dictionary<Regime, HashSet<Waypoint>>();
    }
    public void Calculate(LogicWriteKey key, Alliance alliance)
    {
        if (key.Data.Context.ControlledAreas
                .TryGetValue(alliance, out var controlled) == false)
        {
            GD.Print("no control areas for alliance at poly " 
                     + alliance.Leader.Entity(key.Data).GetPolys(key.Data).First().Id);
            return;
        }
        CalculateAreasOfResponsibility(controlled, key.Data);
    }

    private void CalculateAreasOfResponsibility(IEnumerable<Waypoint> controlled, 
        Data d)
    {
        AreasOfResponsibility = _alliance.Members.Items(d)
            .ToDictionary(r => r, r => new HashSet<Waypoint>());
        var disputed = new HashSet<Waypoint>();
        foreach (var waypoint in controlled)
        {
            var wpMemberOwnerRegimes = waypoint.AssocPolys(d)
                .Select(p => p.OwnerRegime.Entity(d))
                .Where(r => _alliance.Members.Contains(r));
            var wpMemberOccupierRegimes = waypoint.AssocPolys(d)
                .Select(p => p.OccupierRegime.Entity(d))
                .Where(r => _alliance.Members.Contains(r));
            var relRegimes = wpMemberOccupierRegimes
                .Union(wpMemberOwnerRegimes).Distinct();
            if (relRegimes.Count() == 0)
            {
                disputed.Add(waypoint);
                continue;
            }
            foreach (var r in relRegimes)
            {
                AreasOfResponsibility[r].Add(waypoint);
            }
        }
        //todo handle wps of disputed polys where alliance has 
        //forces but not 'occupied' yet
    }
}