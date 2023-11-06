using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public static class NodeExt
{
    public static SubscribedNodeToken SubscribeUpdate(this Node node, Action update,
        params RefAction[] triggers)
    {
        return SubscribedNodeToken.Construct(node, update, triggers);
    }

    public static HBoxContainer MakeFlowStatDisplay(Client client, Flow flow, Data data, float height,
        params RefAction[] triggers)
    {
        var h = flow.Icon.MakeIconStatDisplay(client, data,
            () =>
            {
                var regime = data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(data);
                var f = regime.Flows.Get(flow);
                // return $"In: {f.FlowIn} \n Out: {f.FlowOut} \n Net: {f.Net()}";
                return $"{f.Net()}";
            },
            height, triggers);
        var tooltipTemplate = new FlowTooltipTemplate();
        h.RegisterTooltip(tooltipTemplate, 
            () => (flow, data.BaseDomain.PlayerAux.LocalPlayer.Regime.Entity(data)));
        return h;
    }

    public static void RegisterTooltip<T>(this Control c, TooltipTemplate<T> template, Func<T> getObject)
    {
        var hash = c.GetHashCode();
        c.MouseEntered += () =>
        {
            Game.I.Client.GetComponent<TooltipManager>().PromptTooltip(template, getObject(), hash);
        };
        c.MouseExited += () =>
        {
            Game.I.Client.GetComponent<TooltipManager>().HideTooltip(hash);
        };
        c.MouseFilter = Control.MouseFilterEnum.Stop;
    }
    public static HBoxContainer MakeIconStatDisplay(this Icon icon, 
        Client client,
        Data data, 
        Func<string> getStat, float height,
        params RefAction[] triggers)
    {
        var hBox = new HBoxContainer();
        var amount = new Label();
        var iconRect = icon.GetTextureRect(Vector2.One * height);
        iconRect.CustomMinimumSize = iconRect.Size;
        hBox.AddChild(iconRect);
        hBox.AddChild(amount);
        iconRect.Scale = new Vector2(1f, -1f);
        var stat = StatLabel.Construct<string>(
            client,
            "", 
            amount,
            getStat
        );
        hBox.SubscribeUpdate(stat.TriggerUpdate, triggers);

        return hBox;
    }
    public static HBoxContainer MakeStatDisplay(
        Client client,
        Data data, 
        Func<string> getStat, float height,
        params RefAction[] triggers)
    {
        var hBox = new HBoxContainer();
        var amount = new Label();
        hBox.AddChild(amount);
        var stat = StatLabel.Construct<string>(
            client,
            "", 
            amount,
            getStat
        );
        hBox.SubscribeUpdate(stat.TriggerUpdate, triggers);

        return hBox;
    }
    public static List<Node> GetChildList(this Node n)
    {
        var l = new List<Node>();
        var children = n.GetChildren();
        foreach (var child in children)
        {
            l.Add((Node)child);
        }

        return l;
    }
    public static void AssignChildNode<T>(this Node n, ref T node, string name) where T : Node
    {
        node = (T) n.FindChild(name);
        if (node == null) throw new Exception();
    }
    public static void ChildAndCenterOn(this Node2D parent, Control toCenter, Vector2 parentDim)
    {
        parent.AddChild(toCenter);
        toCenter.Position = -parentDim / 2f;
    }

    public static Label CreateLabel(string text)
    {
        var l = new Label();
        l.Text = text;
        return l;
    }
    public static Label CreateLabelAsChild(this Node parent, string text)
    {
        var label = new Label();
        label.Text = text;
        parent.AddChild(label);
        return label;
    }
    public static void AddToChildWithName(this Node self, Node toAdd, string childName)
    {
        self.FindChild(childName).AddChild(toAdd);
    }
    
    public static bool Toggle(this Node2D n)
    {
        n.Visible = n.Visible == false;
        return n.Visible;
    }

    public static void ClearChildren(this Node n)
    {
        if (n == null) throw new Exception();
        while (n.GetChildCount() > 0)
        {
            var c = n.GetChild(0);
            n.RemoveChild(c);
            c.QueueFree();
        }
    }
    public static void AddChildWithVSeparator(this Node parent, Node n)
    {
        parent.AddChild(n);
        var s = new VSeparator();
        parent.AddChild(s);
    }

    public static int GetTotalNumberOfNodesInSubTree(this Node head)
    {
        var res = 1;
        foreach (var c in head.GetChildren())
        {
            res += c.GetTotalNumberOfNodesInSubTree();
        }
        return res;
    }
}