
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    public Dictionary<Regime, HashSet<Waypoint>> AreasOfResponsibility { get; private set; }
    public AllianceMilitaryAi()
    {
        AreasOfResponsibility = new Dictionary<Regime, HashSet<Waypoint>>();
    }
    public void Calculate(LogicWriteKey key, Alliance alliance)
    {
        if (key.Data.Get<Alliance>(alliance.Id) == null)
        {
            throw new Exception();
        }
        if (key.Data.Context.ControlledAreas
                .TryGetValue(alliance, out var controlled))
        {
            CalculateAreasOfResponsibility(alliance, controlled, key.Data);
        }
        else
        {
            GD.Print("no control areas for alliance at poly " 
                  + alliance.Leader.Entity(key.Data).GetPolys(key.Data).First().Id);
        }
    }

    public void CalculateAreasOfResponsibility(Alliance alliance, IEnumerable<Waypoint> controlled, 
        Data d)
    {
        AreasOfResponsibility = alliance.Members.Items(d)
            .ToDictionary(r => r, 
                r => new HashSet<Waypoint>());
        var disputed = new HashSet<Waypoint>();
        var uncovered = controlled.ToHashSet();
        foreach (var waypoint in controlled)
        {
            var wpMemberOwnerRegimes = waypoint.AssocPolys(d)
                .Select(p => p.OwnerRegime.Entity(d))
                .Where(r => alliance.Members.Contains(r));
            var wpMemberOccupierRegimes = waypoint.AssocPolys(d)
                .Select(p => p.OccupierRegime.Entity(d))
                .Where(r => alliance.Members.Contains(r));
            var relRegimes = wpMemberOccupierRegimes
                .Union(wpMemberOwnerRegimes).Distinct();
            if (relRegimes.Count() == 0)
            {
                GD.Print("no member has control");
                disputed.Add(waypoint);
            }
            foreach (var r in relRegimes)
            {
                if (r == null) throw new Exception();
                AreasOfResponsibility[r].Add(waypoint);
                uncovered.Remove(waypoint);
            }
        }

        if (disputed.Count() > 0) throw new Exception();
        if(uncovered.Count() != 0 ) throw new Exception();
    }
}