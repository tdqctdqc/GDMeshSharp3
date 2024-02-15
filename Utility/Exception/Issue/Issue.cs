
using Godot;

public abstract class Issue
{
    public string Message { get; set; }
    public Vector2 Pos { get; private set; }
    public abstract void Draw(Client c);

    protected Issue(Vector2 pos, string message)
    {
        Pos = pos;
        Message = message;
    }
}