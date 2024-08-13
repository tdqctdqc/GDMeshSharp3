
using System;
using Godot;

public class PolyMode : UiMode
{
    public DefaultSettingsOption<MapPolygon> Poly { get; private set; }
    public DefaultSettingsOption<Cell> Cell { get; private set; }
    private MapOverlayDrawer _mouseOverlay;
    private MeshInstance2D _selectedCell, _selectedPoly;
    private MouseOverHandler _mouseOverHandler;

    public PolyMode(Client client) : base(client, "Poly")
    {
        _mouseOverHandler = new MouseOverHandler(client.Data);
        
        Poly = new DefaultSettingsOption<MapPolygon>("Poly", null);
        Cell = new DefaultSettingsOption<Cell>("Cell", null);
        client.Notices.Selecting.Subscribe(v => SelectCell(v.GetCell(client.Data)));
        Poly.SettingChanged.Subscribe(v =>
        {
            var segmenter = client.GetComponent<MapGraphics>()
                .Segmenter;
            if (v.newVal is not null)
            {
                var mb = new MeshBuilder();
                mb.DrawPolygon(v.newVal.BoundaryPoints, 
                    Colors.White.Tint(.25f));
                mb.DrawCellsBordersInsetLocal(v.newVal.GetCells(client.Data),
                    Colors.Transparent, Colors.Black, 
                    2.5f, 0f, v.newVal.Center, client.Data);
                _selectedPoly.Mesh = mb.GetMesh();
                segmenter.AddElement(_selectedPoly, v.newVal.Center);
            }
            else
            {
                _selectedPoly.Mesh = null;
            }
        });
        
        Cell.SettingChanged.Subscribe(v =>
        {
            var segmenter = client.GetComponent<MapGraphics>()
                .Segmenter;
            if (v.newVal is not null)
            {
                var mb = new MeshBuilder();
                mb.DrawPolygon(v.newVal.RelBoundary, 
                    Colors.Yellow.Tint(.25f));
                mb.DrawBorderInset(Colors.Transparent, Colors.Red, 
                    5f, 0f, v.newVal.RelBoundary);
                _selectedCell.Mesh = mb.GetMesh();
                segmenter.AddElement(_selectedCell, v.newVal.RelTo);
            }
            else
            {
                _selectedCell.Mesh = null;
            }
        });
    }

    public override void Process(float delta)
    {
        _mouseOverHandler.Process(delta);
    }
    
    public override void HandleInput(InputEvent e)
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        _mouseOverlay.Clear();
        _mouseOverHandler.Highlight(_mouseOverlay);
        if(e.IsAction("Open Regime Overview"))
        {
            var cell = _mouseOverHandler.MouseOverCell;
            if (cell.Controller.Get(_client.Data)
                is Regime r)
            {
                RegimeOverviewWindow.Open(r, _client);
            }
        }
        if (e is InputEventMouseButton mb
            && mb.ButtonIndex == MouseButton.Left
            && mb.Pressed == false)
        {
            SelectCell(_mouseOverHandler.MouseOverCell);
        }
    }

    private void SelectCell(Cell cell)
    {
        if (_client.UiController.Mode != this) return;
        Cell.Set(cell);
        if (cell is IPolyCell pc)
        {
            Poly.Set(pc.Polygon.Get(_client.Data));
        }
        else if (cell is IEdgeCell ec)
        {
            var edge = ec.Edge.Get(_client.Data);
            var globalMousePos = _client.Cam().GetMousePosInMapSpace();
            var hi = edge.HighPoly.Get(_client.Data);
            var lo = edge.LowPoly.Get(_client.Data);
            if (hi.PointInPolyAbs(globalMousePos, _client.Data))
            {
                Poly.Set(hi);
            }
            else if (lo.PointInPolyAbs(globalMousePos, _client.Data))
            {
                Poly.Set(lo);
            }
            else
            {
                throw new Exception();
            }
        }
    }

    public override void Enter()
    {
        _mouseOverlay = _client.GetComponent<MapGraphics>()
            .GetOverlay(LayerOrder.Debug);
        _selectedCell?.QueueFree();
        _selectedCell = new MeshInstance2D();
        _selectedCell.ZIndex = (int)LayerOrder.Highlighter;
        _selectedCell.ZAsRelative = false;
        
        _selectedPoly?.QueueFree();
        _selectedPoly = new MeshInstance2D();
        _selectedPoly.ZIndex = (int)LayerOrder.Highlighter;
        _selectedPoly.ZAsRelative = false;
    }

    public override void Tooltip()
    {
        var mapPos = _client.Cam().GetMousePosInMapSpace();
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
        if (_mouseOverHandler.MouseOverPoly != null
            && _mouseOverHandler.MouseOverCell != null)
        {
            var template = new PolyTooltipTemplate();
            tooltip.Prompt(template, 
                (_mouseOverHandler.MouseOverPoly, 
                    _mouseOverHandler.MouseOverCell));
        }
        else
        {
            tooltip.Clear();
        }
    }

    public override void Clear()
    {
        var tooltip = _client.GetComponent<TooltipManager>();
        tooltip.Clear();
        
        _selectedCell.QueueFree();
        _selectedCell = null;
        _selectedPoly.QueueFree();
        _selectedPoly = null;
        
        _mouseOverlay.Clear();
        _client.GetComponent<MapGraphics>().RemoveOverlay(_mouseOverlay);
        _mouseOverlay = null;
    }
}