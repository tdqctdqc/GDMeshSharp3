using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Godot;

public partial class TooltipManager : Control, IClientComponent
{
    private TooltipPanel _panel;
    private object _element;
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

    public void Clear()
    {
        _panel.Visible = false;
    }
    public void Process(float delta)
    {
        if(_element != null) _panel.Move(GetLocalMousePosition() + _offsetFromMouse);
    }
    public void Prompt<TElement>
        (TooltipTemplate<TElement> template, TElement element)
    {
        _panel.Visible = true;
        _element = element;
        
        _panel.Setup(template.GetFastContainer(element, _data),
            template.GetSlowContainer(element, _data), 
            element, _data);
    }
    public void Prompt<TElement>
        (VBoxContainer fast, VBoxContainer slow, TElement element)
    {
        _panel.Visible = true;
        _element = element;
        _panel.Setup(fast, slow, element, _data);
    }

    public void HideTooltip(object element)
    {
        if (_element == element)
        {
            _panel.Visible = false;
            _element = null;
        }
    }
}