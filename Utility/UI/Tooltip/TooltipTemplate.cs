using System;
using System.Collections.Generic;
using System.Linq;
using Godot;


public abstract class TooltipTemplate<T> : ITooltipTemplate
{
    protected abstract List<Func<T, Data, Control>> _fastGetters { get; }
    protected abstract List<Func<T, Data, Control>> _slowGetters { get; }
    protected TooltipTemplate()
    {
    }
    public List<Control> GetFastEntries(T t, Data d)
    {
        return _fastGetters.Select(f => f(t, d)).Where(c => c != null).ToList();
    }

    public List<Control> GetSlowEntries(T t, Data d)
    {
        return _slowGetters.Select(f => f(t, d)).Where(c => c != null).ToList();
    }
    List<Control> ITooltipTemplate.GetFastEntries(object o, Data d) => GetFastEntries((T) o, d);
    List<Control> ITooltipTemplate.GetSlowEntries(object o, Data d) => GetSlowEntries((T) o, d);
}
