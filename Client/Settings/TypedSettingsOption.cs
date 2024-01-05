
using System.Collections.Generic;
using System.Linq;
using Godot;

public class TypedSettingsOption<T> : SettingsOption<T>
{
    private List<T> _options;
    private Dictionary<T, string> _names;
    public TypedSettingsOption(string name, List<T> options, 
        List<string> names) 
        : base(name, options.First())
    {
        _options = options;
        _names = new Dictionary<T, string>();
        for (var i = 0; i < options.Count; i++)
        {
            _names.Add(options[i], names[i]);
        }
    }

    public void Choose<TOption>() where TOption : T
    {
        var first = _options.First(t => t is TOption);
        Set(first);
    } 
    public override Control GetControlInterface()
    {
        var list = new ItemList();
        var token = ItemListToken.Construct(list);
        token.Setup<T>(
            _options,
            t => _names[t],
            t => () => Set(t)
        );
        SettingChanged.Subscribe(t => list.Select(_options.IndexOf(t.newVal)));
        list.Select(0);
        return list;
    }
}