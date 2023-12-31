
using Godot;

public abstract class Issue
{
    public string Message { get; set; }
    public Vector2 Point { get; private set; }
    public abstract void Draw(Client c);

    protected Issue(Vector2 point, string message)
    {
        Point = point;
        Message = message;
    }
}