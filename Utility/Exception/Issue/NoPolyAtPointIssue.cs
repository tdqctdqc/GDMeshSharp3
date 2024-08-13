using Godot;


public class NoPolyAtPointIssue<TPoly> : Issue
{
    public TPoly FoundPoly { get; set; }
    public NoPolyAtPointIssue(Vector2 unitPos, TPoly foundPoly,
        string message, int tick) : base(unitPos, message, tick)
    {
        FoundPoly = foundPoly;
    }

    
}