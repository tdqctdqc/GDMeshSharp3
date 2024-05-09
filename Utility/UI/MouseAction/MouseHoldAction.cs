using System;
using Godot;

public abstract class MouseHoldAction : IMouseAction
{
    protected abstract void MouseDown(InputEventMouse m);
    protected abstract void MouseHeld(InputEventMouse m);
    protected abstract void MouseUp(InputEventMouse m);
    public abstract void Highlight(Client c);
    private MouseButtonMask _button;
    private bool _mouseDown;

    protected MouseHoldAction(MouseButtonMask button)
    {
        _button = button;
        _mouseDown = false;
    }
    public void Process(InputEventMouse m)
    {
        var pressed = Pressed(m);
        if (_mouseDown && pressed)
        {
            MouseHeld(m);
        }
        else if(_mouseDown && pressed == false)
        {
            _mouseDown = false;
            MouseUp(m);
        }
        else if (_mouseDown == false && pressed)
        {
            _mouseDown = true;
            MouseDown(m);
        }
        else if(_mouseDown == false && pressed == false)
        {
        }
    }

    private bool Pressed(InputEventMouse m)
    {
        return (m.ButtonMask & _button) != 0;
    }
}