
using System;
using System.Collections.Generic;
using Godot;

public partial class CustomClickArea : Node2D, IUiCollidable
{
    public List<Vector2[]> RelBoundaries { get; private set; }
    public List<Action> Actions { get; private set; }
    public Vector2 RelTo { get; private set; }
    public int Z { get; private set; }
    private MouseButtonMask _button;
    public IEnumerable<Vector2[]> RelPolygonBoundaries => RelBoundaries;

    public CustomClickArea(MouseButtonMask button,
        Vector2 relTo,
        LayerOrder z)
    {
        _button = button;
        Z = (int)z;
        RelTo = relTo;
        RelBoundaries = new List<Vector2[]>();
        Actions = new List<Action>();
    }
    private CustomClickArea()
    {
    }

    public void Clear()
    {
        RelBoundaries.Clear();
        Actions.Clear();
    }
    public void Add(Vector2[] relBoundary, Action action)
    {
        RelBoundaries.Add(relBoundary);
        Actions.Add(action);
    }

    public void SetRelTo(Vector2 relTo)
    {
        RelTo = relTo;
    }
    public void Handle(InputEvent e, Vector2 pos, Client c)
    {
        if (e is not InputEventMouseButton m
            || Pressed(m) == false)
        {
            return;
        }
        
        var offset = RelTo.Offset(pos, c.Data);
        for (var i = RelBoundaries.Count - 1; i >= 0; i--)
        {
            if (Geometry2D.IsPointInPolygon(offset, RelBoundaries[i]))
            {
                Actions[i].Invoke();
                return;
            }
        }

        throw new Exception();
    }

    public bool IsCapturing()
    {
        return Visible;
    }

    public bool Captures(InputEvent e)
    {
        return e is InputEventMouse m;
    }
    
    protected bool Pressed(InputEventMouseButton e)
    {
        return (e.ButtonMask & _button) != 0; 
    }
}