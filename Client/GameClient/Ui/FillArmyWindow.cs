
using Godot;

public partial class FillArmyWindow : Window
{
    private VBoxContainer _inner;
    public FillArmyWindow()
    {
        this.MakeCloseable();
        _inner = this.MakeScrollContainer<VBoxContainer>(
            new Vector2I(500, 800));
        
    }

    public void Setup(Army army)
    {
        
    }
}