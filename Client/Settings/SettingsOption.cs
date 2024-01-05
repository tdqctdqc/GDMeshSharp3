using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class SettingsOption<T> : ISettingsOption
{
    public string Name { get; private set; }
    public T Value { get; private set; }
    public RefAction<(T oldVal, T newVal)> SettingChanged { get; private set; }
    RefAction ISettingsOption.SettingChanged => SettingChanged.Blank;
    public abstract Control GetControlInterface();

    protected SettingsOption(string name, T value)
    {
        Name = name;
        Value = value;
        SettingChanged = new RefAction<(T, T)>();
    }

    protected void Set(T val)
    {
        if (Value.Equals(val)) return;
        var oldVal = Value;
        Value = val;
        SettingChanged.Invoke((oldVal, val));
    }
}
