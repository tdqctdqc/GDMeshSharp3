using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GenGraphics : WholeMapGraphic
{
    private Action _draw;
    private GenData _data;
    public GenGraphics(GraphicsSegmenter segmenter,
        GenData d)
    {
        segmenter.AddElement(this, Vector2.Zero);
        _data = d;
        ZIndex = 99;
        ZAsRelative = false;
        _draw = DrawPlates;
        Draw();
    }

    private void Draw()
    {
        this.ClearChildren();
        _draw.Invoke();
    }

    private void DrawPlates()
    {
        var mb = new MeshBuilder();
        foreach (var plate in _data.GenAuxData.Plates)
        {
            var col = Colors.Blue.GetPeriodicShade(plate.Id);
            col = new Color(col.R, col.G, col.B, .5f);
            foreach (var cell in plate.Cells)
            {
                foreach (var poly in cell.Polys)
                {
                    foreach (var preCell in _data.GenAuxData.PreCellPolys[poly])
                    {
                        mb.DrawPolygon(preCell.PointsRel.Select(p => preCell.RelTo + p).ToArray(), col);
                    }
                }
            }
        }
        
        AddChild(mb.GetMeshInstance());
    }
    public override Settings GetSettings()
    {
        var settings = new Settings(nameof(GenGraphics));
        var visibility = new BoolSettingsOption(
            "Visible", false);
        visibility.SettingChanged.Subscribe(v => this.Visible = v.newVal);

        var mode = new ListSettingsOption<Action>(
        "Mode", 
            new List<Action>
            {
                DrawPlates
            },
            new List<string>
            {
                "Plates"
            }
        );
        mode.SettingChanged.Subscribe(a => _draw = a.newVal);


        settings.SettingsOptions.Add(visibility);
        settings.SettingsOptions.Add(mode);
        
        return settings;
    }
}