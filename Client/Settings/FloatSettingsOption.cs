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
        var hbox = new VBoxContainer();
        var l = new Label();
        l.Text = Name + ": " + Value.ToString().PadDecimals(2);
        hbox.AddChild(l);
        var slider = new HSlider();
        slider.MinValue = Min;
        slider.MaxValue = Max;
        slider.Step = Step;
        slider.Rounded = Integer;
        slider.Value = Value;
        hbox.AddChild(slider);
        slider.ValueChanged += t =>
        {
            SetProtected((float)t);
            l.Text = Name + ": " + Value.ToString().PadDecimals(2);
        };
        SettingChanged.Subscribe(v => slider.Value = v.newVal);
        return hbox;
    }
    
}
