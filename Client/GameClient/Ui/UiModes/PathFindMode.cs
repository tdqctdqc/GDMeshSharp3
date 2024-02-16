
using Godot;

public class PathFindMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;
    private PolyCell _from;
    private PolyCell _to;
    public PathFindMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
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

    private void DrawPath()
    {
        var debug = _client.GetComponent<MapGraphics>()
            .DebugOverlay;
        debug.Clear();

        if (_from != null)
        {
            debug.Draw(mb => mb.AddPoint(Vector2.Zero, 
                    20f, Colors.Red),
                _from.GetCenter());
        }

        if (_to != null)
        {
            debug.Draw(mb => mb.AddPoint(Vector2.Zero, 
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

        var alliance = _from.Controller.Entity(_client.Data).GetAlliance(_client.Data);
        
        
        var stratMove = _client.Data.Models.MoveTypes.StrategicMove;
        var path = PathFinder.FindPath(stratMove, alliance,
            _from, _to, _client.Data);
        if (path == null)
        {
            return;
        }
        for (var i = 0; i < path.Count - 1; i++)
        {
            var from = path[i];
            var to = path[i + 1];
            var offset = from.GetCenter().GetOffsetTo(to.GetCenter(), _client.Data);
            debug.Draw(mb => mb.AddArrow(Vector2.Zero, offset, 5f, Colors.Yellow),
                from.GetCenter());
        }
    }

    public override void Clear()
    {
        var debug = _client.GetComponent<MapGraphics>()
            .DebugOverlay;
        debug.Clear();
    }
}