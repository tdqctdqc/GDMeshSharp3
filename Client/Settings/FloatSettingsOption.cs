using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class FloatSettingsOption : SettingsOption<float>
{
    public float Min { get; private set; }
    public float Max { get; private set; }
    public float Step { get; private set; }
    public bool Integer { get; private set; }

    public FloatSettingsOption(string name, float value, float min, 
        float max, float step, bool integer) : base(name, value)
    {
        Min = min;
        Max = max;
        Step = step;
        Integer = integer;
    }

    public override Control GetControlInterface()
    {
        var ctrl = new NumSliderAndEntry(Name, Value, Min, Max, Step);
        ctrl.ValueChanged += t =>
        {
            Set(t);
        };
        SettingChanged.Subscribe(v => ctrl.SetValue(v.newVal));
        return ctrl;
    }
    
}
