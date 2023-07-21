using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class UiRequests 
{
    public RefFunc<Type, Window> OpenWindowRequest { get; private set; }
    public RefAction<PolyTriPosition> MouseOver { get; private set; }
    public RefAction<ITooltipInstance> PromptTooltip { get; private set; }
    public RefAction<ITooltipInstance> HideTooltip { get; private set; }
    public RefAction<string> ToggleMapGraphicsLayer { get; private set; }

    public UiRequests()
    {
        OpenWindowRequest = new RefFunc<Type, Window>();
        MouseOver = new RefAction<PolyTriPosition>();
        PromptTooltip = new RefAction<ITooltipInstance>();
        HideTooltip = new RefAction<ITooltipInstance>();
        ToggleMapGraphicsLayer = new RefAction<string>();
    }
    public TWindow OpenWindow<TWindow>() where TWindow : Window
    {
        var w = OpenWindowRequest?.Invoke(typeof(TWindow));
        return (TWindow) w;
    }
}
