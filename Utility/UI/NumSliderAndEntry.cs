
using System;
using Godot;

public partial class NumSliderAndEntry : HBoxContainer
{
    public float Value { get; private set; }
    public float MinValue { get; private set; }
    public float MaxValue { get; private set; }
    public event Action<float> ValueChanged;
    private HSlider _slider;
    private SpinBox _spin;
    public NumSliderAndEntry(string name,
        float value,
        float min, float max, float step)
    {
        if (Mathf.Clamp(value, min, max) != value) throw new Exception();
        Value = value;
        
        _slider = new HSlider();
        _slider.ExpandFill();
        _slider.CustomMinimumSize = new Vector2(50f, 0f);
        _slider.Step = step;
        _slider.Value = Value;
        _spin = new SpinBox();
        _spin.Step = step;
        _spin.Value = Value;
        
        SetRange(min, max);

        _slider.ValueChanged += v =>
        {
            _spin.Value = v;
            Value = (float)v;
            ValueChanged?.Invoke((float)v);
        };
        
        _spin.ValueChanged += v =>
        {
            _slider.Value = v;
            Value = (float)v;
            ValueChanged?.Invoke((float)v);
        };
        _spin.UpdateOnTextChanged = true;
        this.CreateLabelAsChild(name);
        AddChild(_spin);
        AddChild(_slider);
    }

    public void SetRange(float min, float max)
    {
        Value = Mathf.Clamp(Value, min, max);
        MaxValue = max;
        MinValue = min;
        _slider.MinValue = min;
        _slider.MaxValue = max;
        _spin.MinValue = min;
        _spin.MaxValue = max;
    }

    public void SetValue(float value)
    {
        Value = Mathf.Clamp(value, MinValue, MaxValue);
        
        if (value == Value) return;
        _slider.Value = value;
        _spin.Value = value;
        Value = value;
        ValueChanged?.Invoke(value);
    }
}