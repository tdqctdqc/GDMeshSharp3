
using Godot;

public class DefaultSettingsOption<T> : SettingsOption<T>
{
    public DefaultSettingsOption(string name, T value) : base(name, value)
    {
    }

    public override Control GetControlInterface()
    {
        var c = new Control();
        return c;
    }

}