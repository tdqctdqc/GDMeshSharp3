
using System.Collections.Generic;
using Godot;

public interface IGraphicLayer
{
    List<ISettingsOption> Settings { get; }
    Control GetControl();
    string Name { get; }
    void Update(Data d);
    bool Visible { get; set; }
}