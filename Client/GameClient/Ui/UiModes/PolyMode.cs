
using Godot;

public class PolyMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;

    public PolyMode(Client client) : base(client)
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
        _client.HighlightPoly(_mouseOverHandler.MouseOverPoly);
        Tooltip(mapPos);
    }
    private void Tooltip(Vector2 mapPos)
    {
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
        if (_mouseOverHandler.MouseOverPoly != null
            && _mouseOverHandler.MouseOverCell != null)
        {
            var template = new PolyTooltipTemplate();
            _client.GetComponent<TooltipManager>()
                .PromptTooltip(template, (_mouseOverHandler.MouseOverPoly, _mouseOverHandler.MouseOverCell));
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
}