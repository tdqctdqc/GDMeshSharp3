
using System;
using System.Collections.Generic;
using System.Linq;

public static class IdCountExt
{
    public static IEnumerable<KeyValuePair<T, float>> 
        GetEnumModel<T>(this IdCount<T> count, Data d)
            where T : IModel
    {
        return count.Contents.Select(kvp => 
            new KeyValuePair<T, float>((T)d.Models.GetModel<IModel>(kvp.Key), 
                kvp.Value));
    }
    
    public static IEnumerable<KeyValuePair<TSub, float>> 
        GetEnumerableModelOfType<T, TSub>(this IdCount<T> count, Data d)
        where TSub : T where T : IModel
    {
        return count.GetEnumModel(d).Where(kvp => kvp.Key is TSub)
            .Select(kvp => new KeyValuePair<TSub, float>((TSub)kvp.Key, kvp.Value));
    }
    
    
    public static IEnumerable<KeyValuePair<T, float>> 
        GetEnumEntity<T>(this IdCount<T> count, Data d)
        where T : Entity
    {
        return count.Contents.Select(kvp => 
            new KeyValuePair<T, float>((T)d.Get<T>(kvp.Key), 
                kvp.Value));
    }
}