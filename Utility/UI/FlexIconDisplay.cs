using System.Collections.Generic;
using Godot;

public partial class FlexIconDisplay : Control
{
    public List<Control> Children { get; private set; }
    private Control _control;
    private Vector2 _iconDim;
    private Vector2 _maxSize;
    private float _margin;

    public FlexIconDisplay(Control control, 
        Vector2 iconDim, Vector2 maxSize)
    {
        _control = control;
        _control.Resized += SetMaxSize;
        
        _iconDim = iconDim;
        _maxSize = maxSize;
        
        SetMaxSize();
    }

    public override void _ExitTree()
    {
        _control.Resized -= SetMaxSize;
    }

    public void SetChildren(List<Control> children)
    {
        Children = children;
        foreach (var control in Children)
        {
            AddChild(control);
        }
        Arrange();
    }


    public void SetMaxSize()
    {
        _maxSize = new Vector2(_control.Size.X * .8f, _maxSize.Y);
        Arrange();
    }
    public void SetMaxSize(Vector2 maxSize)
    {
        _maxSize = maxSize;
        Arrange();
    }
    public void Arrange()
    {
        if (Children is null)
        {
            Size = Vector2.Zero;
            CustomMinimumSize = Vector2.Zero;
            return;
        }
        var fitColumns = Mathf.FloorToInt(_maxSize.X / _iconDim.X) 
                         - 2;
        var fitRows = Mathf.FloorToInt(_maxSize.Y / _iconDim.Y) 
                      - 2;
        fitColumns = Mathf.Max(1, fitColumns);
        fitRows = Mathf.Max(1, fitRows);
        var fit = fitColumns * fitRows;
        Vector2 bounds;
        if (Children.Count <= fit)
        {
            var boundsX = 0f;
            var boundsY = 0f;
            var row = 0;
            var column = 0;
            for (var i = 0; i < Children.Count; i++)
            {
                var c = Children[i];
                var y = row * _iconDim.Y;
                boundsY = Mathf.Max(y, boundsY);
                
                var x = column * _iconDim.X;
                boundsX = Mathf.Max(x, boundsX);
                c.Position = new Vector2(x, y);
                
                column++;
                if (column >= fitColumns)
                {
                    column = 0;
                    row++;
                }
            }

            bounds = new Vector2(boundsX, boundsY) + _iconDim;
        }
        else
        {
            var numInRow = Mathf.CeilToInt(Children.Count / fitRows);
            var xWidth = _maxSize.X / numInRow;

            for (var i = 0; i < Children.Count; i++)
            {
                var column = i % numInRow;
                var row = i / numInRow;
                var c = Children[i];
                var y = row * _iconDim.Y;
                var x = column * xWidth;
                c.Position = new Vector2(x, y);
            }

            bounds = _maxSize;
        }

        CustomMinimumSize = bounds;
        Size = bounds;
    }
}