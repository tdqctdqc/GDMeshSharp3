
using System;
using Godot;

public partial class StatLabel : SubscribedNodeToken
{
    private Label _label;
    private Action _update;

    public static StatLabel Construct<TProperty>(
        string prefix, Label label,
        Func<TProperty> getStat,
        params RefAction[] triggers)
    {
        var s = new StatLabel();
        s.Setup(prefix, label, getStat, triggers);
        return s;
    }
    protected void Setup<TProperty>(
        string prefix, Label label,
        Func<TProperty> getStat,
        params RefAction[] triggers)
    {
        _label = label;
        if (_label == null) throw new Exception();
        Action update = () =>
        {
            if (prefix != "")
            {
                _label.Text = $"{prefix}: {getStat().ToString()}";
            }
            else
            {
                _label.Text = $"{getStat().ToString()}";
            }
        };
        
        base.Setup(_label, update, triggers);
    }
}
