using System;
using System.Collections.Generic;
using System.Linq;

public class ValChangeAction<TEntity, TVal> : RefAction<ValChangeNotice<TEntity, TVal>> where TEntity : Entity
{
    public void Invoke(TEntity entity, TVal newVal, TVal oldVal)
    {
        Invoke(new ValChangeNotice<TEntity, TVal>(entity, newVal, oldVal));
    }
}
