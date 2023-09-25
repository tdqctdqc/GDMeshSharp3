
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AttributeHolder<T> : IReadOnlyList<T> where T : IGameAttribute
{
    public T[] Attributes { get; private set; }

    public AttributeHolder(params T[] attributes)
    {
        Attributes = attributes;
    }

    public bool Has<TAttr>() where TAttr : T
    {
        return Attributes.Any(a => a is TAttr);
    }

    public TAttr Get<TAttr>() where TAttr : T
    {
        return (TAttr)Attributes.First(a => a is TAttr);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Attributes.AsEnumerable<T>().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return Attributes.GetEnumerator();
    }

    public int Count => Attributes.Length;

    public T this[int index] => Attributes[index];
}