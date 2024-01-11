
using System;
using System.Linq;
using Godot;

public class UnitMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public UnitMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler();
    }
    public override void Process(float delta)
    {
    }
    
    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        mapPos = mapPos.ClampPosition(_client.Data);
        Game.I.Client.Cam().Process(e);
        _mouseOverHandler.Process(_client.Data, mapPos);

        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverPoly);
        }
        Highlight(mapPos);
        var debug = _client.GetComponent<MapGraphics>().DebugOverlay;
        debug.Clear();
        var u = GetCloseUnit(mapPos);
        if(u != null)
        {
            UnitTooltip(u);
            OverlayForUnit(u);
        }
        else
        {
            _client.GetComponent<MapGraphics>().DebugOverlay.Clear();
        }
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        mg.DebugOverlay.Clear();
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
    }

    private void Highlight(Vector2 mapPos)
    {
        _client.HighlightPoly(_mouseOverHandler.MouseOverPoly);
    }

    private Unit GetCloseUnit(Vector2 mapPos)
    {
        var grid = _client.Data.Military.UnitAux.UnitGrid;
        var within = grid.GetWithin(mapPos, 50f);
        return within
            .OrderBy(u => u.Position.Pos.GetOffsetTo(mapPos, _client.Data).Length())
            .FirstOrDefault(u => mapPos.GetOffsetTo(u.Position.Pos, _client.Data).Length() <= u.Radius());
    }
    private void UnitTooltip(Unit close)
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.DebugOverlay.Clear();
        var tooltip = _client.GetComponent<TooltipManager>();
        if (close != null)
        {
            tooltip.PromptTooltip(new UnitTooltipTemplate(), close);
        }
        else
        {
            tooltip.Clear();
        }
    }

    private void OverlayForUnit(Unit u)
    {
        var unitPos = u.Position.Pos;
        var debug = _client.GetComponent<MapGraphics>().DebugOverlay;
        var group = u.GetGroup(_client.Data);
        var groupMembers = group.Units.Items(_client.Data);
        foreach (var gUnit in groupMembers)
        {
            if (gUnit == u) continue;
            var radius = gUnit.Radius();
            var offset = u.Position.Pos.GetOffsetTo(gUnit.Position.Pos, _client.Data);
            debug.Draw(mb => mb.AddPoint(offset, radius * 2f, new Color(Colors.Red, .5f)),
                unitPos);
        }
        debug.Draw(mb => mb.DrawMovementRecord(u.Id,
                50, unitPos, _client.Data),
            unitPos);
        var order = group.GroupOrder;
        if (order != null)
        {
            debug.Draw(mb => order.Draw(group, unitPos, mb, _client.Data), unitPos);
        }
    }
    
}