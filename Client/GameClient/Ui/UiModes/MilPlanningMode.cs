
using System.Linq;
using Godot;

public class MilPlanningMode : UiMode
{
    private MouseOverHandler _mouseOverHandler;

    public MilPlanningMode(Client client) : base(client)
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        _mouseOverHandler.ChangedCell += c => DrawAllianceTheaters();
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
    }
    private void DrawAllianceTheaters()
    {
        var mg = _client.GetComponent<MapGraphics>();
        var debug = mg.DebugOverlay;
        debug.Clear();
        var cell = _mouseOverHandler.MouseOverCell;
        if (cell == null) return;
        if (cell.Controller.IsEmpty()) return;
        var regime = cell.Controller.Entity(_client.Data);
        if (regime.IsPlayerRegime(_client.Data)) return;
        var alliance = regime.GetAlliance(_client.Data);
        var ai = _client.Data.HostLogicData.AllianceAis[alliance];
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
                        debug.Draw(mb => mb.DrawPolygon(c.RelBoundary,
                                new Color(Colors.Black, .5f)),
                            c.RelTo);
                    }
                }
                debug.Draw(mb => mb.DrawFrontFaces(frontline.Faces, 
                    Colors.Black, 3f, pos, _client.Data), pos);


                if (frontline.AdvanceFront != null)
                {
                    debug.Draw(mb => mb.DrawFrontFaces(
                        frontline.AdvanceFront, 
                        Colors.White, 2f, pos, _client.Data), pos);
                }

                if (frontline.SalientFronts != null)
                {
                    int iter = 0;
                    foreach (var salient in frontline.SalientFronts)
                    {
                        debug.Draw(mb => mb.DrawFrontFaces(
                            salient, 
                            ColorsExt.GetRainbowColor(iter++),
                            1f, pos, _client.Data), pos);
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