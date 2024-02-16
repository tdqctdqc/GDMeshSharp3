
using System.Linq;
using Godot;

public class MilPlanningMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;

    public MilPlanningMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedCell += c => DrawRegimeTheaters();
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
    }
    private void DrawRegimeTheaters()
    {
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        debug.Clear();
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null) return;
        if (cell.Controller.IsEmpty()) return;
        var regime = cell.Controller.Entity(_client.Data);
        var alliance = regime.GetAlliance(_client.Data);
        var ai = _client.Data.HostLogicData.RegimeAis[regime];
        var relTo = regime.GetPolys(_client.Data).First().Center;
        
        foreach (var theater in ai.Military.Strategic.Theaters)
        {
            foreach (var frontline in theater.Frontlines)
            {
                var pos = frontline.Faces.First().GetNative(_client.Data).GetCenter();
                if (frontline.AdvanceInto != null)
                {
                    foreach (var c in frontline.AdvanceInto)
                    {
                        debug.Draw(mb => mb.DrawPolygon(c.RelBoundary, Colors.Orange),
                            c.RelTo);
                    }
                }
                debug.Draw(mb => mb.DrawFrontFaces(frontline.Faces, 
                    Colors.Black, 3f, pos, _client.Data), pos);


                if (frontline.AdvanceLines != null)
                {
                    for (var i = 0; i < frontline.AdvanceLines.Count; i++)
                    {
                        var color = ColorsExt.GetRainbowColor(i);
                        debug.Draw(mb => mb.DrawFrontFaces(
                            frontline.AdvanceLines[i], 
                            color, 1f, pos, _client.Data), pos);
                    }
                }
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