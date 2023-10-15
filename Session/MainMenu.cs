using System;
using System.Collections.Generic;
using System.Linq;using Godot;

public partial class MainMenu : Node
{
    public Client Client { get; }

    public MainMenu()
    {
        var startScene = SceneManager.Instance<StartScene>();
        AddChild(startScene);
    }

    public void Setup()
    {
    }
}
