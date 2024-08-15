
using Godot;

public class PathFindMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    private MapOverlayDrawer _cellOverlay, _pathOverlay;
    private Cell _from;
    private Cell _to;
    public PathFindMode(Client client) 
        : base(client, "Path Find")
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
        _cellOverlay.Clear();
        _mouseOverHandler.Highlight(_cellOverlay);
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb && mb.Pressed == false)
        {
            var cell = _mouseOverHandler.MouseOverCell;
            if (cell == null) return;
            if (mb.ButtonIndex == MouseButton.Left)
            {
                _from = cell;
            }
            else if (mb.ButtonIndex == MouseButton.Right)
            {
                _to = cell;
            }
            DrawPath();
        }
    }

    public override void Enter()
    {
        var mg = _client.GetComponent<MapGraphics>();
        _pathOverlay = mg.GetOverlay(LayerOrder.Highlighter);
        _cellOverlay = mg.GetOverlay(LayerOrder.Highlighter);
    }

    private void DrawPath()
    {
        if (_from != null)
        {
            _pathOverlay.Draw(mb => mb.AddSquare(Vector2.Zero, 
                    20f, Colors.Red),
                _from.GetCenter());
        }

        if (_to != null)
        {
            _pathOverlay.Draw(mb => mb.AddSquare(Vector2.Zero, 
                    20f, Colors.Green),
                _to.GetCenter());
        }
        
        if (_from == null || _to == null)
        {
            return;
        }

        if (_from.Controller.IsEmpty())
        {
            return;
        }

        var regime = _from.Controller.Get(_client.Data);
        
        
        var stratMove = _client.Data.Models.MoveTypes.StrategicMove;
        var path = PathFinder.FindPathThroughFriendly(stratMove, regime,
            _from, _to, _client.Data);
        if (path == null)
        {
            return;
        }
        for (var i = 0; i < path.Count - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];
            var offset = from.GetCenter().Offset(to.GetCenter(), _client.Data);
            _pathOverlay.Draw(mb => mb.AddArrow(Vector2.Zero, offset, 5f, Colors.Yellow),
                from.GetCenter());
        }
    }

    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.RemoveOverlay(_pathOverlay);
        mg.RemoveOverlay(_cellOverlay);
    }
}