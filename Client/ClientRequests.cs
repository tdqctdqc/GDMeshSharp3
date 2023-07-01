using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ClientRequests
{
    private EntityTypeTree _tree;
    public RefFunc<Type, Window> OpenWindowRequest { get; private set; }
    public RefAction<PolyTriPosition> MouseOver { get; private set; }
    public RefAction<ITooltipInstance> PromptTooltip { get; private set; }
    public RefAction<ITooltipInstance> HideTooltip { get; private set; }
    public RefAction<Command> QueueCommand { get; private set; }
    public ClientRequests(ISession session)
    {
        OpenWindowRequest = new RefFunc<Type, Window>();
        MouseOver = new RefAction<PolyTriPosition>();
        PromptTooltip = new RefAction<ITooltipInstance>();
        HideTooltip = new RefAction<ITooltipInstance>();
        QueueCommand = new RefAction<Command>();
        QueueCommand.Subscribe(session.Server.QueueCommandLocal);
    }
    public void GiveTree(EntityTypeTree tree)
    {
        _tree = tree;
    }

    public TWindow OpenWindow<TWindow>() where TWindow : Window
    {
        var w = OpenWindowRequest?.Invoke(typeof(TWindow));
        return (TWindow) w;
    }
}
