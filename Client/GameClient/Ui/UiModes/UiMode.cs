
using Godot;

public abstract class UiMode
{
    protected Client _client;
    public string Name { get; private set; }

    protected UiMode(Client client, string name)
    {
        Name = name;
        _client = client;
    }

    public abstract void Process(float delta);
    public abstract void HandleInput(InputEvent e);
    public abstract void Enter();
    public abstract void Clear();
}