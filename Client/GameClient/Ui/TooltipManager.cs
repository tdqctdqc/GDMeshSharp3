using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public partial class TooltipManager : Control
{
    private TooltipPanel _panel;
    private ITooltipInstance _currInstance;
    private Vector2 _offsetFromMouse = new Vector2(20f, 20f);
    private Data _data;
    public TooltipManager(Data data)
    {
        _data = data;
        _panel = new TooltipPanel();
        AddChild(_panel);
        _panel.Visible = false;
        Game.I.Client.Requests.PromptTooltip.Subscribe(PromptTooltip);
        TreeExiting += () => Game.I.Client.Requests.PromptTooltip.Unsubscribe(PromptTooltip);
        Game.I.Client.Requests.HideTooltip.Subscribe(HideTooltip);
        TreeExiting += () => Game.I.Client.Requests.HideTooltip.Unsubscribe(HideTooltip);;
    }

    private TooltipManager()
    {
        
    }
    public void Process(float delta, Vector2 mousePosInMapSpace)
    {
        _panel.Move(GetLocalMousePosition() + _offsetFromMouse);
    }

    private void PromptTooltip(ITooltipInstance instance)
    {
        _panel.Visible = true;
        _panel.Setup(instance, _data);
        _currInstance = instance;
    }

    private void HideTooltip(ITooltipInstance instance)
    {
        if (_currInstance == instance)
        {
            _panel.Visible = false;
            _currInstance = null;
        }
    }
}