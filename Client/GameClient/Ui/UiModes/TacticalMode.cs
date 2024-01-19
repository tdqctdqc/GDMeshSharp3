
using System.Linq;
using Godot;

public class TacticalMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public TacticalMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler();
    }

    public override void Process(float delta)
    {
    }

    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        Game.I.Client.Cam().HandleInput(e);
        _mouseOverHandler.Process(_client.Data, mapPos);
        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverPoly);
        }
        
        var debugDrawer 
            = Game.I.Client.GetComponent<MapGraphics>()
                .DebugOverlay;
        debugDrawer.Clear();
        
        DrawFrontSegment();
        Tooltip(mapPos);
    }

    public override void Clear()
    {
    }
    
    private void DrawFrontSegment()
    {
        var debugDrawer 
            = Game.I.Client.GetComponent<MapGraphics>()
                .DebugOverlay;
        if (_mouseOverHandler.MouseOverCell == null)
        {
            return;
        }
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell.Controller.Empty()) return;
        var regime = cell.Controller.Entity(_client.Data);
        if (regime.IsPlayerRegime(_client.Data)) return;
        if (_client.Logic is HostLogic h == false) return;
        var ready = h.OrderHolder.Orders.ContainsKey(regime);
        if (ready == false) return;
        var ai = _client.Data.HostLogicData.RegimeAis[regime];
        var deployment = ai.Military.Deployment;
        foreach (var theater in deployment.ForceAssignments.OfType<TheaterAssignment>())
        {
            foreach (var front in theater.Assignments.OfType<FrontAssignment>())
            {
                foreach (var seg in front.Assignments.OfType<FrontSegmentAssignment>())
                {
                    var center = seg.GetCells(_client.Data).First().GetCenter();
                    debugDrawer.Draw(mb => mb.DrawFrontSegment(
                        center,
                        seg, _client.Data
                        ), center);
                }
            }
        }
    }

    private void Tooltip(Vector2 mapPos)
    {
        
    }
}