
using Godot;

public abstract class UiMode
{
    protected Client _client;

    protected UiMode(Client client)
    {
        _client = client;
    }

    public abstract void Process(float delta);
    public abstract void HandleInput(InputEvent e);
}