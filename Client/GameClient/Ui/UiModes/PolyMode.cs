
using Godot;

public class PolyMode : UiMode
{
    public DefaultSettingsOption<MapPolygon> Poly { get; private set; }
    public DefaultSettingsOption<Cell> Cell { get; private set; }
    private MouseOverHandler _mouseOverHandler;

    public PolyMode(Client client) : base(client, "Poly")
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        Poly = new DefaultSettingsOption<MapPolygon>("Poly", null);
        Cell = new DefaultSettingsOption<Cell>("Cell", null);
        _mouseOverHandler.ChangedCell += Cell.Set;
        _mouseOverHandler.ChangedPoly += Poly.Set;
        _mouseOverHandler.ChangedCell += c => Highlight();
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }
    
    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        if(e.IsAction("Open Regime Overview"))
        {
            _client.TryOpenRegimeOverview(_mouseOverHandler.MouseOverCell);
        }
        
        Tooltip(mapPos);
    }

    public override void Enter()
    {
        
    }

    private void Highlight()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.Highlighter.Clear();
        _mouseOverHandler.Highlight();
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