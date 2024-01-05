
using Godot;

public abstract class Issue
{
    public string Message { get; set; }
    public Vector2 UnitPos { get; private set; }
    public abstract void Draw(Client c);

    protected Issue(Vector2 unitPos, string message)
    {
        UnitPos = unitPos;
        Message = message;
    }
}