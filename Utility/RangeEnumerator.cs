
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RangeEnumerator<T> : IEnumerator<T>
{
    private List<T> _list;
    private int _index, _from, _to, _length;
    public RangeEnumerator(List<T> list, int from, int to, int length)
    {
        _list = list;
        Current = list[from];
        _index = 0;
        _from = from;
        _to = to;
        _length = length;
    }

    public bool MoveNext()
    {
        _index++;
        if (_index >= _length)
        {
            return false;
        }

        Current = _list[ (_from + _index) % _list.Count ];
        return true;
    }

    public void Reset()
    {
        _index = -1;
    }

    public T Current { get; set; }
    object IEnumerator.Current { get; }
    public void Dispose()
    {
        
    }
}
