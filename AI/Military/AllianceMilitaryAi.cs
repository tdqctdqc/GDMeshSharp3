
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
        CalculateAreasOfResponsibility(alliance, key.Data);
    }
    
    public void CalculateAreasOfResponsibility(Alliance alliance, Data d)
    {
        if (d.Context.ControlledAreas
                .TryGetValue(alliance, out var controlled)
                    == false)
        {
            GD.Print("no control areas for alliance at poly " 
                     + alliance.Leader.Entity(d).GetPolys(d).First().Id);
            return;
        }
        
        AreasOfResponsibility = alliance.Members.Items(d)
            .ToDictionary(r => r, r => new HashSet<Waypoint>());
        var disputed = new HashSet<Waypoint>();
        var uncovered = controlled.ToHashSet();
        foreach (var waypoint in controlled)
        {
            Regime r = null;
            if (waypoint.GetOccupyingRegime(d) is Regime regime
                && regime.GetAlliance(d) == alliance)
            {
                r = regime;
            }
            else
            {
                var fb = waypoint.GetForceBalance(d);
                var haveUnits = fb.ByRegime
                    .Where(kvp => alliance.Members.Contains(kvp.Key));
                if (haveUnits.Count() > 0)
                {
                    r = haveUnits.MaxBy(kvp => kvp.Value).Key;
                }
                else
                {
                    GD.Print("couldnt find responsible");
                }
            }
            uncovered.Remove(waypoint);

            if (r == null)
            {
                GD.Print("no member has control");
                disputed.Add(waypoint);
            }
            else
            {
                AreasOfResponsibility[r].Add(waypoint);
            }
        }

        // if (disputed.Count() > 0) throw new Exception();
        if (uncovered.Count() != 0 ) throw new Exception();
    }
}