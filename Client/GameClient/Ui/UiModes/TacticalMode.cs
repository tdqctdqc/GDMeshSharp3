
using System.Linq;
using Godot;

public class TacticalMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public TacticalMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedCell += c => DrawFrontSegment();
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverPoly);
        }
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        Tooltip(mapPos);
    }

    public override void Clear()
    {
        var debugDrawer 
            = Game.I.Client.GetComponent<MapGraphics>()
                .DebugOverlay;
        debugDrawer.Clear();
    }
    
    private void DrawFrontSegment()
    {
        var debugDrawer 
            = Game.I.Client.GetComponent<MapGraphics>()
                .DebugOverlay;
        debugDrawer.Clear();
        if (_mouseOverHandler.MouseOverCell == null)
        {
            return;
        }
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell.Controller.IsEmpty()) return;
        var regime = cell.Controller.Entity(_client.Data);
        if (regime.IsPlayerRegime(_client.Data)) return;
        if (_client.Logic is HostLogic h == false) return;
        var ready = h.OrderHolder.Orders.ContainsKey(regime);
        if (ready == false) return;
        var ai = _client.Data.HostLogicData.RegimeAis[regime];
        var deployment = ai.Military.Deployment;
        foreach (var theater in deployment.Root.Branches.OfType<Theater>())
        {
            foreach (var seg in theater.Branches.OfType<FrontSegment>())
            {
                var center = seg.GetCells(_client.Data).First().GetCenter();
                debugDrawer.Draw(mb => mb.DrawFrontSegment(
                    center,
                    seg, _client.Data
                    ), center);
            }
        }
    }

    private void Tooltip(Vector2 mapPos)
    {
        
    }
}