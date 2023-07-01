using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ButtonToken : Node
{
    private List<Action> _actions;
    public Button Button { get; private set; }

    public static Button CreateButton(string label, params Action[] actions)
    {
        var button = new Button();
        CreateToken(button, actions);
        button.Text = label;
        return button;
    }
    public static ButtonToken FindButtonCreateToken(Node node, string buttonName, params Action[] actions)
    {
        var button = (Button) node.FindChild(buttonName);
        var token = new ButtonToken();
        token.Setup(button, actions);
        return token;
    }
    public static ButtonToken CreateToken(Button button, params Action[] actions)
    {
        var token = new ButtonToken();
        token.Setup(button, actions);
        return token;
    }
    private ButtonToken()
    {
        
    }
    public void Setup(Button button, params Action[] actions)
    {
        Button = button;
        _actions = actions.ToList();
        //todo re-subscribing itself?
        button.ButtonUp += OnButtonUp;
        // if(button.IsConnected("button_up", this, nameof(OnButtonUp)) == false)
        // {
        //     button.Connect("button_up", this, nameof(OnButtonUp));
        // }
        var p = GetParent();
        if (p != button)
        {
            p?.RemoveChild(this);
            button.AddChild(this);
        }
    }
    private void OnButtonUp()
    {
        for (int i = 0; i < _actions.Count; i++)
        {
            _actions[i].Invoke();
        }
    }
}