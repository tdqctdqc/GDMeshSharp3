
using Godot;
using Ui.MilitaryWindow;
public partial class MilitaryWindow : TabWindow
{
    public Regime Regime { get; set; }
    private MilitaryWindow(Client c) : base(c)
    {
        MinSize = new Vector2I(1000, 1000);
        var makeUnits = new MakeUnitsTab(this);
        makeUnits.Name = "Make Units";
        AddTab(makeUnits);

        var armies = new ArmiesTab(this);
        armies.Name = "Armies";
        AddTab(armies);
    }

    public static MilitaryWindow Get(Regime r, Client client)
    {
        var w = new MilitaryWindow(client);
        w.Regime = r;
        return w;
    }
}