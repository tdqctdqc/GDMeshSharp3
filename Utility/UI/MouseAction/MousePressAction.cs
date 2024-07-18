using Godot;
using MathNet.Numerics;


public abstract class MousePressAction : IMouseAction
{
    private bool _pressed;
    private MouseButtonMask _button;
    protected abstract void MouseUp(InputEventMouse m);

    public MousePressAction()
    {
        _pressed = false;
    }
    public void Process(InputEventMouse m)
    {
        if(_pressed && Pressed(m) == false)
        {
            MouseUp(m);
            _pressed = false;
        }
        else if(Pressed(m))
        {
            _pressed = true;
        }
    }

    public abstract void Highlight(Client c, MapOverlayDrawer overlay);
    

    protected MousePressAction(MouseButtonMask button)
    {
        _button = button;
    }

    protected bool Pressed(InputEventMouse e)
    {
        return (e.ButtonMask & _button) != 0; 
    }
}