using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class Ui : CanvasLayer
{
    protected Dictionary<Type, Window> _windows;
    protected Ui()
    {
        
    }

    protected void Setup(IClient client)
    {
        _windows = new Dictionary<Type, Window>();
        client.UiRequests.OpenWindowRequest.Subscribe(type =>
        {
            _windows[type].PopupCentered();
            return _windows[type];
        });
    }
    protected void AddWindow(Window window)
    {
        _windows.Add(window.GetType(), window);
        AddChild(window);
        window.Hide();
    }
}
