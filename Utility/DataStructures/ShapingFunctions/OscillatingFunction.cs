using Godot;

public class OscillatingFunction : IFunction<float, float>
{
    private float _period, _max, _min;

    public OscillatingFunction(float period, float max, float min)
    {
        _period = period;
        _max = max;
        _min = min;
    }

    public float Calc(float t)
    {
        if (t == 0f) return _max;
        var v = Mathf.Cos(t * Mathf.Pi * 2f / _period) * (_max - _min) + _max;
        return Mathf.Min(_max, v);
    }
}