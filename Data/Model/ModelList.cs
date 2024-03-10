using System;
using System.Collections.Generic;
using System.Linq;

public abstract class ModelList<T> where T : IModel
{
    public List<T> GetList()
    {
        return GetType().GetProperties()
            .Where(m => typeof(T).IsAssignableFrom(m.PropertyType))
            .Select(m => (T)m.GetValue(this)).ToList();
    }
}
