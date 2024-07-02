
using Godot;
using Ui.Combat;

public partial class CombatSimWindow : TabWindow
{
    public CombatSimWindow(Client c)
        : base(c)
    {
        Size = new Vector2I(1000, 800);
        var sim = new SimTab(c);
        AddTab(sim);
    }

    public static void Open(Client client)
    {
        var w = new CombatSimWindow(client);
        Game.I.Client.WindowHolder.OpenWindow(w);
    }
}