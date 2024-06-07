
using Godot;
using Ui.MilitaryWindow;
public partial class MilitaryWindow : TabWindow
{
    public Regime Regime { get; set; }
    public MilitaryWindow(Client c) : base(c)
    {
        MinSize = new Vector2I(1000, 1000);
        var makeUnits = new MakeUnitsTab();
        makeUnits.Name = "Make Units";
        AddTab(makeUnits);

        var armies = new ArmiesTab(this);
        armies.Name = "Armies";
        AddTab(armies);
    }
}