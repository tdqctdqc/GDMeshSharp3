
using System.Linq;
using Godot;

public class MilPlanningMode : UiMode
{
    private MouseOverHandler _mouseOver;
    private DefaultSettingsOption<Alliance> _alliance;
    private MapOverlayDrawer _cellOverlay, _plansOverlay;

    public MilPlanningMode(Client client) : base(client,
        "MilitaryPlanning")
    {
        _mouseOver = new MouseOverHandler(client.Data);
        _mouseOver.ChangedCell += c => 
        {
            _cellOverlay.Clear();
            _mouseOver.Highlight(_cellOverlay);
        };
        _alliance = new DefaultSettingsOption<Alliance>("Alliance", null);
        _alliance.SettingChanged.Subscribe(n =>
        {
            DrawRegimePlans();
        });
    }

    public override void Process(float delta)
    {
        _mouseOver.Process(delta);
    }

    public override void HandleInput(InputEvent e)
    {
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Left
            && mb.Pressed == false)
        {
            var cell = _mouseOver.MouseOverCell;
            _alliance.Set(cell.Controller.Get(_client.Data).GetAlliance(_client.Data));
        }
    }

    public override void Enter()
    {
        var mg = _client.GetComponent<MapGraphics>();
        _plansOverlay = mg.GetOverlay(LayerOrder.Highlighter);
        _cellOverlay = mg.GetOverlay(LayerOrder.Highlighter);
    }


    private void DrawRegimePlans()
    {
        _plansOverlay.Clear();
        var alliance = _alliance.Value;
        if (alliance is null) return;
        var ai = _client.Data.HostLogicData.AllianceAis[alliance];
        var relTo = alliance.Leader.Get(_client.Data).GetCells(_client.Data).First().GetCenter();
        if (ai.Military.Strategic.Theaters == null) return;
        foreach (var theater in ai.Military.Strategic.Theaters.Entities(_client.Data))
        {
            foreach (var frontline in theater.Frontlines.Entities(_client.Data))
            {
                var pos = frontline.Faces.First().GetNative(_client.Data).GetCenter();
                if (frontline.AdvanceInto != null)
                {
                    foreach (var c in frontline.AdvanceInto)
                    {
                        var cell = c.Get(_client.Data);
                        _plansOverlay.Draw(mb => mb.DrawPolygon(cell.RelBoundary,
                                new Color(Colors.Black, .5f)),
                            cell.RelTo);
                    }
                }
                _plansOverlay.Draw(mb => mb.DrawFrontFaces(frontline.Faces, 
                    Colors.Black, 3f, pos, _client.Data), pos);
            }
        }
    }
    public override void Clear()
    {
        var mg = _client.GetComponent<MapGraphics>();
        mg.RemoveOverlay(_plansOverlay);
        mg.RemoveOverlay(_cellOverlay);
    }
}