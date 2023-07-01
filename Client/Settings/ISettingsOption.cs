using System;
using Godot;

public interface ISettingsOption
{
    string Name { get; }
    RefAction SettingChanged { get; }
    Control GetControlInterface();
}
