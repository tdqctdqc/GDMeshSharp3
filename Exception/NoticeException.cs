using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class NoticeException : DisplayableException
{
    public string Message { get; private set; }
    public NoticeException(string message)
    {
        Message = message;
    }
    public override Node2D GetGraphic()
    {
        var n = new Node2D();
        n.CreateLabelAsChild(Message);
        return n;
    }
    public override Control GetUi()
    {
        return NodeExt.CreateLabel(Message);
    }
}
