
using System;
using Godot;

public partial class StatLabel : SubscribedNodeToken
{
    private Label _label;
    private Action _update;

    public static StatLabel Construct<TProperty>(
        IClient client,
        string prefix, Label label,
        Func<TProperty> getStat,
        params RefAction[] triggers)
    {
        var s = new StatLabel();
        s.Setup(client, prefix, label, getStat, triggers);
        return s;
    }
    protected void Setup<TProperty>(
        IClient client,
        string prefix, Label label,
        Func<TProperty> getStat,
        params RefAction[] triggers)
    {
        _label = label;
        if (_label == null) throw new Exception();
        Action update = () =>
        {
            client.QueuedUpdates.Enqueue(() =>
            {
                if (prefix != "")
                {
                    _label.Text = $"{prefix}: {getStat().ToString()}";
                }
                else
                {
                    _label.Text = $"{getStat().ToString()}";
                }
            });
        };
        
        base.Setup(_label, update, triggers);
    }
}
