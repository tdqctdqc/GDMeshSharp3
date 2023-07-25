
using Godot;

public interface IGraphicLayer
{
    Control GetControl();
    string Name { get; }
    void Update(Data d);
    bool Visible { get; set; }
}