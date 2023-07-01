
using System.Collections;
using System.Collections.Generic;

public class ListRange<T> : IEnumerable<T>
{
    private List<T> _list;
    private int _from, _to, _length;
    public T this[int i] => _list[_from + i];
    public int Count => _to - _from + 1;
    public ListRange(List<T> list, int from, int count)
    {
        _list = list;
        _from = from;
        _to = (from + count) % _list.Count;
        if (_from < _to) _length = _to - _from + 1;
        else _length = _to + 1 + (list.Count - _from);
    }
    public ListRange(ListRange<T> list, int from, int count)
    {
        _list = list._list;
        _from = from;
        _to = (from + count) % _list.Count;

        if (_from < _to) _length = _to - _from + 1;
        else _length = _to + 1 + (list.Count - _from);
    }
    public ListRange<T> GetRange(int start, int count)
    {
        return new ListRange<T>(_list, _from + start, count);
    }
    public IEnumerator<T> GetEnumerator()
    {
        return new RangeEnumerator<T>(_list, _from, _to, _length);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
