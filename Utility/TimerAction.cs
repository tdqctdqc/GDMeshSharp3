
using System;

public class TimerAction
{
    private float _timerPeriod, _timer;
    private Action _action;
    private bool _oneTime;

    public TimerAction(float timerPeriod, float timer, Action action, bool oneTime = false)
    {
        _oneTime = oneTime;
        _timerPeriod = timerPeriod;
        _timer = timer;
        _action = action;
    }

    public TimerAction(float timerPeriod)
    {
        _timerPeriod = timerPeriod;
        _timer = timerPeriod;
    }

    public bool Process(double delta)
    {
        _timer += (float)delta;
        if (_timer >= _timerPeriod)
        {
            _timer = 0f;
            _action?.Invoke();
            if (_oneTime) _action = null;
            return true;
        }

        return false;
    }

    public void ResetTimer()
    {
        _timer = 0f;
    }

    public void SetAction(Action action)
    {
        _action = action;
    }
}
