using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

public partial class WindowManager : Node, IClientComponent
{
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }
    public void Process(float delta)
    {
        
    }

    protected Dictionary<Type, Window> _windows;
    public WindowManager(Client client)
    {
        _windows = new Dictionary<Type, Window>();
       AddWindow(SceneManager.Instance<PromptWindow>());
       AddWindow(LoggerWindow.Get());
       AddWindow(new EntityOverviewWindow());
       client.UiLayer.AddChild(this);
    }
    public void AddWindow(Window window)
    {
        _windows.Add(window.GetType(), window);
        AddChild(window);
        window.Hide();
    }

    public void RemoveWindow(Window window)
    {
        var w = _windows[window.GetType()];
        _windows.Remove(window.GetType());
        w.QueueFree();
    }

    public T OpenWindow<T>() where T : Window
    {
        var type = typeof(T);
        _windows[type].PopupCentered();
        return (T)_windows[type];
    }
    public T GetWindow<T>() where T : Window
    {
        return (T)_windows[typeof(T)];
    }
}
