using System;
using System.Collections.Generic;
using System.Linq;

public class ValChangeAction<TVal> : RefAction<ValChangeNotice<TVal>>
{
    public void Invoke(Entity entity, TVal newVal, TVal oldVal)
    {
        Invoke(new ValChangeNotice<TVal>(entity, newVal, oldVal));
    }
}
