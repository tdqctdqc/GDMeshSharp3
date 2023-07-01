using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class EnumSettingsOption<TEnum> : SettingsOption<TEnum> where TEnum : Enum
{
    public EnumSettingsOption(string name, TEnum value) : base(name, value)
    {
    }
    public override Control GetControlInterface()
    {
        var allVals = Enum.GetNames(Value.GetType());
        var list = new ItemList();
        var token = ItemListToken.Construct(list);
        token.Setup<int>(
            Enumerable.Range(0, allVals.Count()).ToList(),
            i => allVals[i],
            i => () => { Set((TEnum) Enum.Parse(typeof(TEnum), allVals[i])); }
        );
        return list;
    }
}
