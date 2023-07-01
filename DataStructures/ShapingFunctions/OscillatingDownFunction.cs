
using Godot;

public class OscillatingDownFunction : IFunction<float, float>
{
    private float _period, _max, _min, _shrinkFactor;

    public OscillatingDownFunction(float period, float max, float min, float halfingDist)
    {
        _shrinkFactor = 2f / halfingDist;
        _period = period;
        _max = max;
        _min = min;
    }

    public float Calc(float t)
    {
        if (t == 0f) return _max;
        var v = (Mathf.Cos(t * Mathf.Pi * 2f / _period) * (_max - _min) + (_max - _min) + _min) / (t * _shrinkFactor);
        return Mathf.Min(_max, v);
    }
}
