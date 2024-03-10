using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Godot;


public class DeploymentMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public DeploymentMode(Client client) 
        : base(client, "Deployment")
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedCell += c => Draw();
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
    }

    public override void Enter()
    {
        
    }

    private void Draw()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        
        if (_mouseOverHandler.MouseOverCell == null)
        {
            return;
        }
        _mouseOverHandler.Highlight();
        if (_mouseOverHandler.MouseOverCell.Controller
            .IsEmpty())
        {
            return;
        }
        mg.DebugOverlay.Clear();
        var alliance = _mouseOverHandler.MouseOverCell.Controller
            .Entity(_client.Data).GetAlliance(_client.Data);
        DrawAllianceBorders(alliance);
        if (alliance.Leader.Entity(_client.Data).IsPlayerRegime(_client.Data))
        {
            return;
        }
        DrawAllianceAi(alliance);
    }
    private void DrawAllianceAi(Alliance alliance)
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
        var ai = _client.Data.HostLogicData.AllianceAis[alliance];
        var root = ai.Military.Deployment.GetRoot();
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        if (root != null)
        {
            DrawDeploymentBranch(root);
            var theaters = root.SubBranches.OfType<TheaterBranch>();
            var segs = theaters
                .SelectMany(t => t.Assignments.OfType<HoldLineAssignment>());
            foreach (var seg in segs)
            {
                var pos = seg.GetCharacteristicCell(_client.Data).GetCenter();
                debug.Draw(mb => mb.DrawFrontAssignment(pos,
                    seg, _client.Data), pos);
            }
        }
    }

    

    private Vector2 DrawDeploymentBranch(DeploymentBranch branch)
    {
        var alliance = branch.Alliance;
        var leader = alliance.Leader.Entity(_client.Data);
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        var template = new DeploymentBranchTooltipTemplate();

        var graphic = NodeExt.GetTooltipTrigger(branch.GetType().Name,
            template, branch);
        var node = new Node2D();
        node.AddChild(graphic);
        var pos = branch.GetMapPosForDisplay(_client.Data);
        debug.AddNode(node, pos);
        foreach (var child in branch.SubBranches)
        {
            var childPos = DrawDeploymentBranch(child);
            debug.Draw(mb => mb.AddLine(Vector2.Zero,
                    pos.Offset(childPos, _client.Data), leader.GetMapColor(),
                    5f), 
                pos);
        }
        foreach (var ga in branch.Assignments)
        {
            var childPos = DrawGroupAssignment(ga);
            debug.Draw(mb => mb.AddLine(Vector2.Zero,
                    pos.Offset(childPos, _client.Data), leader.GetMapColor(),
                    5f), 
                pos);
        }

        return pos;
    }

    private Vector2 DrawGroupAssignment(GroupAssignment ga)
    {
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        var template = new GroupAssignmentTooltipTemplate();
        var graphic = NodeExt.GetTooltipTrigger(ga.GetType().Name, template, ga);
        var node = new Node2D();
        node.AddChild(graphic);
        var pos = ga.GetCharacteristicCell(_client.Data).GetCenter();
        debug.AddNode(node, pos);
        return pos;
    }
    
    private void DrawAllianceBorders(Alliance alliance)
    {
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        var cells = _client.Data.Planet.PolygonAux.PolyCells.Cells.Values
            .Where(c => alliance.Members.RefIds.Contains(c.Controller.RefId)).ToArray();
        
        var relTo = alliance.Leader.Entity(_client.Data).GetPolys(_client.Data).First().Center;
        
        foreach (var c in cells)
        {
            for (var i = 0; i < c.Neighbors.Count; i++)
            {
                var n = PlanetDomainExt.GetPolyCell(c.Neighbors[i], _client.Data);
                if (alliance.Members.RefIds.Contains(n.Controller.RefId)) continue;
                var edge = c.Edges[i];
                Color color = Colors.Green;
                if (n.Controller.IsEmpty())
                {
                    color = Colors.Blue;
                }
                else
                {
                    var foreignAlliance = n.Controller.Entity(_client.Data).GetAlliance(_client.Data);
                    if (alliance.IsAtWar(foreignAlliance, _client.Data))
                    {
                        color = Colors.Red;
                    }
                    else if (alliance.IsRivals(foreignAlliance, _client.Data))
                    {
                        color = Colors.Orange;
                    }
                }
                debug.Draw(mb => mb.AddLine(edge.f, edge.t, color, 10f), c.RelTo);
            }
        }
    }
    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
        mg.Highlighter.Clear();
    }
}