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
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedPoly += c => DrawRegime();
        _mouseOverHandler.ChangedCell += c => Highlight();
        _mouseOverHandler.ChangedPoly += c => Highlight();
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
    }

    private void DrawRegime()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
        if (_mouseOverHandler.MouseOverPoly == null)
        {
            return;
        }
        if (_mouseOverHandler.MouseOverPoly.OccupierRegime
            .IsEmpty())
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
        // DrawRegimeLineOrders(regime);
    }

    private void DrawRegimeTheaters(Regime regime)
    {
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        var alliance = regime.GetAlliance(_client.Data);
        var ai = _client.Data.HostLogicData.RegimeAis[regime];
        var relTo = regime.GetPolys(_client.Data).First().Center;
        var theaters = ai.Military.Deployment.Root.Branches.OfType<Theater>();
        var segs = theaters.SelectMany(t => t.Branches.OfType<FrontSegment>());

        foreach (var c in ai.Military
                .Deployment.Root.Children().OfType<DeploymentBranch>())
        {
            drawBranch(c);
        }
        
        Vector2 drawBranch(DeploymentBranch branch)
        {
            var graphic = branch.GetGraphic(_client.Data);
            var node = new Node2D();
            node.AddChild(graphic);
            var pos = branch.GetMapPosForDisplay(_client.Data);
            debug.AddNode(node, pos);
            foreach (var child in branch.Children().OfType<DeploymentBranch>())
            {
                var childPos = drawBranch(child);
                debug.Draw(mb => mb.AddLine(Vector2.Zero,
                    pos.GetOffsetTo(childPos, _client.Data), regime.GetMapColor(),
                    5f), 
                    pos);
            }

            return pos;
        }
        // foreach (var front in fronts)
        // {
        //     debug.Draw(mb => mb.DrawFront(relTo, front, _client.Data), relTo);
        // }
        foreach (var seg in segs)
        {
            debug.Draw(mb => mb.DrawFrontSegment(relTo, seg, _client.Data), relTo);
            foreach (var (groupId, faces) in seg.HoldLine.FacesByGroupId)
            {
                var group = _client.Data.Get<UnitGroup>(groupId);
                var centerFace = faces[faces.Count / 2];
                debug.Label(groupId.ToString(), group.Color, centerFace.GetNative(_client.Data).GetCenter());
            }
        }
    }

    private void DrawRegimeLineOrders(Regime regime)
    {
        var ai = _client.Data.HostLogicData.RegimeAis[regime];
        var groups = _client.Data.Military.UnitAux.UnitGroupByRegime[regime];
        var lineOrders = groups
            .Where(g => g.GroupOrder is DeployOnLineGroupOrder)
            .Select(g => (g, (DeployOnLineGroupOrder)g.GroupOrder));
        var debug = _client.GetComponent<MapGraphics>().DebugOverlay;
        foreach (var pair in lineOrders)
        {
            var group = pair.g;
            var order = pair.Item2;
            var relTo = PlanetDomainExt.GetPolyCell(order.Faces.First().Native, _client.Data)
                .GetCenter();
            debug.Draw(mb => mb.DrawLineOrder(relTo, order, group, _client.Data), relTo);
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
            if (foreign.OccupierRegime.IsEmpty())
            {
                color = Colors.Blue;
            }
            else
            {
                var foreignAlliance = foreign.OccupierRegime.Entity(_client.Data).GetAlliance(_client.Data);
                if (alliance.IsAtWar(foreignAlliance, _client.Data))
                {
                    color = Colors.Red;
                }
                else if (alliance.IsRivals(foreignAlliance, _client.Data))
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
    private void Highlight()
    {
        var highlight = _client.GetComponent<MapGraphics>().Highlighter;
        highlight.Clear();
        if (_mouseOverHandler.MouseOverPoly == null
            || _mouseOverHandler.MouseOverCell == null) return;
        _client.HighlightPoly(_mouseOverHandler.MouseOverPoly);
        _client.HighlightCell(_mouseOverHandler.MouseOverCell);
    }
    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
        mg.Highlighter.Clear();
    }
}