using Godot;


public partial class MapGraphicsControl : Control
{
    private Client _client;
    public MapGraphicsControl(Client c)
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _client = c;
        MouseExited += () =>
        {
            Game.I.Client.GetComponent<TooltipManager>()
                .Clear();
        };
    }
    
    public override void _UnhandledInput(InputEvent e)
    {
        Game.I.Client.UiController.Mode.Tooltip();
    }
}