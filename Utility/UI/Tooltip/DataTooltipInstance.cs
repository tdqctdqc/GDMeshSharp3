using System;
using System.Collections.Generic;
using System.Linq;

public class DataTooltipInstance<T> : ITooltipInstance
{
    public DataTooltipTemplate<T> Template { get; private set; }
    public T Element { get; private set; }

    public DataTooltipInstance(DataTooltipTemplate<T> template, T element)
    {
        Template = template;
        Element = element;
    }
    
    public void SetElement(T element)
    {
        Element = element;
    }
    object ITooltipInstance.Element => Element; 
    IDataTooltipTemplate ITooltipInstance.Template => Template; 
}
