
using Godot;

public class DefaultSettingsOption<T> : SettingsOption<T>
{
    public DefaultSettingsOption(string name, T value) : base(name, value)
    {
    }

    public override Control GetControlInterface()
    {
        return new Control();
    }

    public void Set(T t)
    {
        SetProtected(t);
    }
}