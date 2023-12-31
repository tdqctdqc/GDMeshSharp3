
using Godot;

public class BlankMode : UiMode
{
    public override void Process(float delta)
    {
    }

    public override void HandleInput(InputEvent e)
    {
        _client.Cam().Process(e);
    }

    public BlankMode(Client client) : base(client)
    {
    }
}