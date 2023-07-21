using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class TooltipToken : Node
{
    private IDataTooltipTemplate _template;
    private IClient _client;
    private Action _spawnTooltip;
    private Action _despawnTooltip;
    public static TooltipToken Construct<T>(DataTooltipInstance<T> instance, Control control, Data data)
    {
        var token = new TooltipToken(control,
            () => Game.I.Client.UiRequests.PromptTooltip.Invoke(instance),
            () => Game.I.Client.UiRequests.HideTooltip.Invoke(instance));
        return token;
    }
    
    private TooltipToken(Control control, Action spawnTooltip, Action despawnTooltip)
    {
        _despawnTooltip = despawnTooltip;
        _spawnTooltip = spawnTooltip;
        control.MouseEntered += MouseEnter;
        control.MouseExited += MouseExit;
        control.AddChild(this);
    }

    private TooltipToken()
    {
    }

    private void MouseEnter()
    {
        _spawnTooltip();
    }

    private void MouseExit()
    {
        _despawnTooltip();
    }
}
