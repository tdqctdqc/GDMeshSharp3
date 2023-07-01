using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class SettingsOption<T> : ISettingsOption
{
    public string Name { get; private set; }
    public T Value { get; private set; }
    public RefAction SettingChanged { get; private set; }
    public abstract Control GetControlInterface();

    protected SettingsOption(string name, T value)
    {
        Name = name;
        Value = value;
        SettingChanged = new RefAction();
    }

    protected void Set(T val)
    {
        Value = val;
        SettingChanged.Invoke();
    }
}
