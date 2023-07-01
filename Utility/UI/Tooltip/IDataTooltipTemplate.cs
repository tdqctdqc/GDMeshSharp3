
using System.Collections.Generic;
using Godot;

public interface IDataTooltipTemplate 
{
    List<Control> GetFastEntries(object o, Data d);
    List<Control> GetSlowEntries(object o, Data d);
}
