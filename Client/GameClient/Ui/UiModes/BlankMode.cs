
using Godot;

public class BlankMode : UiMode
{
    public override void Process(float delta)
    {
    }

    public override void HandleInput(InputEvent e)
    {
        _client.Cam().HandleInput(e);
    }

    public override void Enter()
    {
        
    }

    public override void Clear()
    {
        
    }

    public BlankMode(Client client) 
        : base(client, "Blank")
    {
    }
}