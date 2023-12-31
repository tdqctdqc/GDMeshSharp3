using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public partial class TooltipManager : Control, IClientComponent
{
    private TooltipPanel _panel;
    private ITooltipTemplate _currTemplate;
    private Vector2 _offsetFromMouse = new Vector2(20f, 20f);
    private Data _data;
    
    Node IClientComponent.Node => this;
    public Action Disconnect { get; set; }

    public TooltipManager(Data data, Client client)
    {
        _data = data;
        _panel = new TooltipPanel();
        AddChild(_panel);
        _panel.Visible = false;
        client.UiLayer.AddChild(this);
    }
    public void Process(float delta)
    {
        if(_currTemplate != null) _panel.Move(GetLocalMousePosition() + _offsetFromMouse);
    }
    public void PromptTooltip(ITooltipTemplate template, object element)
    {
        _panel.Visible = true;
        _panel.Setup(template, element, _data);
        _currTemplate = template;
    }

    public void HideTooltip(ITooltipTemplate template)
    {
        if (template == _currTemplate)
        {
            _panel.Visible = false;
            _currTemplate = null;
        }
    }
}