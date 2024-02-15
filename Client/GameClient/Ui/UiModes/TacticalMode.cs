
using System.Linq;
using Godot;

public class TacticalMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    public TacticalMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
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

    private void Tooltip(Vector2 mapPos)
    {
        
    }
}