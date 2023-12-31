using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class PromptWindow : ClosableWindow
{
    private Container _container;
    public void Setup(Prompt prompt)
    {
        this.AssignChildNode(ref _container, "Container");
        
        _container.FocusMode = Control.FocusModeEnum.None;
        _container.ClearChildren();
        var descrLabel = new Label();
        descrLabel.Text = prompt.Descr;
        _container.AddChild(descrLabel);
        for (var i = 0; i < prompt.Actions.Count; i++)
        {
            var btn = ButtonExt.GetButton();
            btn.ButtonUp += prompt.Actions[i];
            btn.ButtonUp += () =>
            {
                prompt.Satisfied?.Invoke();
                QueueFree();
            };
            btn.Text = prompt.ActionDescrs[i];
            _container.AddChild(btn);
        }

        CloseRequested += QueueFree;
    }
}