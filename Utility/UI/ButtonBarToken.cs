using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class ButtonBarToken : Node
{
    public Container Container { get; private set; }
    public static ButtonBarToken Create<C>() where C : Container, new()
    {
        var t = new ButtonBarToken();
        t.Setup<C>();
        return t;
    }
    public static T Create<T, C>(T t) where C : Container, new() where T : ButtonBarToken
    {
        t.Setup<C>();
        return t;
    }
    protected ButtonBarToken()
    {
    }

    private void Setup<C>() where C : Container, new()
    {
        Container = new C();
        Container.AddChild(this);
        Container.CustomMinimumSize = new Vector2(0f, 50f);
    }
    public void AddWindowButton<T>(string name) where T : Window
    {
        var settingsWindowBtn 
            = ButtonToken.CreateButton(name, () => Game.I.Client.Requests.OpenWindow<T>());
        Container.AddChild(settingsWindowBtn);
    }

    public Button AddButton(string label, Action action)
    {
        var b = ButtonToken.CreateButton( label, action);
        Container.AddChild(b);
        return b;
    }
}
