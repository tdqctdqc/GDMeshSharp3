using System;
using System.Linq;
using Godot;

public partial class CustomArea : Control
{
    private Vector2[] _triPoints;
    private Vector2 _relTo;
    private Action<InputEventMouse> _mouseAction;
    private MeshInstance2D _mi;
    public static CustomArea Construct(
        MeshInstance2D mi,
        Vector2 relTo,
        Vector2[] triPoints, 
        Action<InputEventMouse> mouseAction)
    {
        var ca = new CustomArea();
        ca._mi = mi;
        ca._relTo = relTo;
        ca._triPoints = triPoints;
        ca._mouseAction = mouseAction;

        if (ca._triPoints.Any())
        {
            var minX = ca._triPoints.Min(v => v.X);
            var maxX = ca._triPoints.Max(v => v.X);
            var minY = ca._triPoints.Min(v => v.Y);
            var maxY = ca._triPoints.Max(v => v.Y);
            var magX = Mathf.Max(Mathf.Abs(minX), Mathf.Abs(maxX));
            var magY = Mathf.Max(Mathf.Abs(minY), Mathf.Abs(maxY));
            ca.Size = new Vector2(magX * 2f, magY * 2f);
        }
        return ca;
    }
    private CustomArea()
    {
        MouseFilter = MouseFilterEnum.Pass;
        this.MouseExited += () => _mi.Modulate = Colors.White;
        GuiInput += e =>
        {
            if (e is InputEventMouse m)
            {
                var mPos = Game.I.Client.Cam().GetMousePosInMapSpace();
                var offset = _relTo.Offset(mPos, 
                    Game.I.Client.Data);
                var minDist = Mathf.Inf;
                for (var i = 0; i < _triPoints.Length; i+=3)
                {
                    var a = _triPoints[i];
                    var b = _triPoints[i+1];
                    var c = _triPoints[i+2];
                    if (TriangleExt.ContainsPoint(a, b, c, 
                            offset))
                    {
                        _mi.Modulate = Colors.Black;
                        _mouseAction(m);
                        GetViewport().SetInputAsHandled();
                        return;
                    }
                }
                _mi.Modulate = Colors.White;
            }
            
        };
    }
}