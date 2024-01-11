using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;


public class StrategicMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    private Regime _regime;
    public StrategicMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler();
    }

    public override void Process(float delta)
    {
        
    }

    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        Game.I.Client.Cam().Process(e);
        _mouseOverHandler.Process(_client.Data, mapPos);
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
        DrawRegime();
    }

    private void DrawRegime()
    {
        if (_mouseOverHandler.MouseOverPoly == null)
        {
            return;
        }
        if (_mouseOverHandler.MouseOverPoly.OccupierRegime
            .Empty())
        {
            return;
        }
        var regime = _mouseOverHandler.MouseOverPoly.OccupierRegime.Entity(_client.Data);
        DrawRegimeBorders(regime);
        if (regime.IsPlayerRegime(_client.Data))
        {
            return;
        }
        DrawRegimeAi(regime);
    }
    private void DrawRegimeAi(Regime regime)
    {
        if (_client.Logic is HostLogic h == false)
        {
            return;
        }
        var ready = h.OrderHolder.GetNumAisReady(_client.Data);
        if (ready.X != ready.Y)
        {
            return;
        }
        
        DrawRegimeTheaters(regime);
    }

    private void DrawRegimeTheaters(Regime regime)
    {
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        var alliance = regime.GetAlliance(_client.Data);
        var ai = _client.Data.HostLogicData.RegimeAis[regime];
        var relTo = regime.GetPolys(_client.Data).First().Center;
        var theaters = ai.Military.Deployment.ForceAssignments.OfType<TheaterAssignment>();
        foreach (var theater in theaters)
        {
            var fronts = theater.Assignments.OfType<FrontAssignment>();
            foreach (var front in fronts)
            {
                var segs = front.Assignments.OfType<FrontSegmentAssignment>();
                foreach (var seg in segs)
                {
                    debug.Draw(mb => mb.DrawFrontSegment(relTo, seg, _client.Data), relTo);
                }
                foreach (var wpId in front.HeldWaypointIds)
                {
                    var wp = MilitaryDomain.GetWaypoint(wpId, _client.Data);
                    debug.Draw(mb => 
                            mb.AddPoint(relTo.GetOffsetTo(wp.Pos, 
                                    _client.Data), 5f, 
                                wp.IsThreatened(alliance, _client.Data)
                                    ? Colors.Orange :  Colors.Green),
                        relTo);
                }
                foreach (var wpId in front.TargetAreaWaypointIds)
                {
                    var wp = MilitaryDomain.GetWaypoint(wpId, _client.Data);
                    debug.Draw(mb => 
                            mb.AddPoint(relTo.GetOffsetTo(wp.Pos, 
                                    _client.Data), 5f, Colors.Red),
                        relTo);
                }
            }
        }
    }

    private void DrawRegimeBorders(Regime regime)
    {
        var alliance = regime.GetAlliance(_client.Data);
        var edges = new HashSet<MapPolygonEdge>();
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        foreach (var p in regime.GetPolys(_client.Data))
        {
            foreach (var e in p.GetEdges(_client.Data))
            {
                if (edges.Contains(e)) edges.Remove(e);
                else edges.Add(e);
            }
        }
        var relTo = regime.GetPolys(_client.Data).First().Center;
        foreach (var e in edges)
        {
            var hi = e.HighPoly.Entity(_client.Data);
            var lo = e.LowPoly.Entity(_client.Data);
            var native = hi.OccupierRegime.RefId == regime.Id
                ? hi
                : lo;
            var foreign = native == hi ? lo : hi;
            if (foreign.OccupierRegime.RefId == regime.Id) throw new Exception();
            
            Color color = Colors.Green;
            if (foreign.OccupierRegime.Empty())
            {
                color = Colors.Blue;
            }
            else
            {
                var foreignAlliance = foreign.OccupierRegime.Entity(_client.Data).GetAlliance(_client.Data);
                if (alliance.AtWar.Contains(foreignAlliance))
                {
                    color = Colors.Red;
                }
                else if (alliance.Rivals.Contains(foreignAlliance))
                {
                    color = Colors.Orange;
                }
            }
            
            foreach (var s in e.GetSegsAbs(_client.Data))
            {
                debug.Draw(mb => mb.AddLine(
                        relTo.GetOffsetTo(s.From, _client.Data),
                        relTo.GetOffsetTo(s.To, _client.Data),
                        color, 10f), 
                    relTo);
            }
        }
    }
    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
    }
}