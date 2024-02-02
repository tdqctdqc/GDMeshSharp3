using System.Collections.Generic;
using System.Linq;

public class IdRecycler
{
    private HashSet<int> _pool, _available;
    
    public IdRecycler()
    {
        _pool = new HashSet<int>();
        _available = new HashSet<int>();
    }
    
    public void Reset()
    {
        _available.AddRange(_pool);
    }

    public int TakeId(Data d)
    {
        if (_available.Count == 0)
        {
            var take = d.IdDispenser.TakeId();
            _pool.Add(take);
            _available.Add(take);
        }
        var id = _available.First();
        _available.Remove(id);
        return id;
    }
}