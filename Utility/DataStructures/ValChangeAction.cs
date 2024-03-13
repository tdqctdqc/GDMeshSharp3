using System;
using System.Collections.Generic;
using System.Linq;

public class ValChangeAction<TOwner, TVal> 
    : RefAction<ValChangeNotice<TOwner, TVal>> 
{
    public void Invoke(TOwner entity, TVal newVal, TVal oldVal)
    {
        Invoke(new ValChangeNotice<TOwner, TVal>(entity, newVal, oldVal));
    }
}
