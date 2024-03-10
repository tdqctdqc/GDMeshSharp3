
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class ListSettingsOption<T> : SettingsOption<T>
{
    public List<T> Options { get; private set; }
    private Dictionary<T, string> _names;
    public ListSettingsOption(string name, List<T> options, 
        List<string> names) 
        : base(name, options.First())
    {
        Options = options;
        _names = new Dictionary<T, string>();
        for (var i = 0; i < options.Count; i++)
        {
            _names.Add(options[i], names[i]);
        }
    }

    public void Choose<TOption>() where TOption : T
    {
        var first = Options.First(t => t is TOption);
        SetProtected(first);
    }

    public void Choose(T t)
    {
        if (Options.Contains(t) == false) throw new Exception();
        SetProtected(t);
    }
    public override Control GetControlInterface()
    {
        var list = new ItemList();
        var token = ItemListToken.Construct(list);
        token.Setup<T>(
            Options,
            t => _names[t],
            t => () => SetProtected(t)
        );
        SettingChanged.SubscribeForNode(t => list.Select(Options.IndexOf(t.newVal)),
            list);
        list.Select(Options.IndexOf(Value));
        list.FocusMode = Control.FocusModeEnum.None;
        return list;
    }
    
    public Control GetControlInterfaceIcon(
        Func<T, Texture2D> getTexture,
        Vector2I iconSize)
    {
        var list = new ItemList();
        list.FixedIconSize = iconSize;
        var token = ItemListToken.Construct(list);
        token.Setup(Options,
            m => _names[m],
            m => { return () =>
            {
                SetProtected(m);
            }; },
            getTexture);
        SettingChanged.SubscribeForNode(t => list.Select(Options.IndexOf(t.newVal)),
            list);
        list.Select(Options.IndexOf(Value));
        list.FocusMode = Control.FocusModeEnum.None;
        return list;
    }
}