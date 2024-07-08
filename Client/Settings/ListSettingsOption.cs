
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
        Set(first);
    }

    public void Choose(T t)
    {
        if (Options.Contains(t) == false) throw new Exception();
        Set(t);
    }
    public override Control GetControlInterface()
    {
        var token = new ItemListToken<T>(
            Options,
            t => _names[t],
            t =>  Set(t)
        );
        var list = token.ItemList;
        SettingChanged.SubscribeForNode(t => list.Select(Options.IndexOf(t.newVal)),
            list);
        list.Select(Options.IndexOf(Value));
        list.FocusMode = Control.FocusModeEnum.None;
        list.ExpandFill();
        list.CustomMinimumSize = new Vector2(100f, 100f);
        return list;
    }
    
    public Control GetControlInterfaceIcon(
        Func<T, Texture2D> getTexture,
        int iconHeight)
    {
        var token = new ItemListToken<T>(
            Options,
            m => _names[m],
            m => Set(m),
            getTexture,
            iconHeight);
        var list = token.ItemList;

        SettingChanged.SubscribeForNode(t => list.Select(Options.IndexOf(t.newVal)),
            list);
        list.Select(Options.IndexOf(Value));
        list.FocusMode = Control.FocusModeEnum.None;
        return list;
    }
}