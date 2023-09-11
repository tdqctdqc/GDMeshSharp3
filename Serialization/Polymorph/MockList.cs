using System.Collections.Generic;

public class MockList<T>
{
    public List<T> Items { get; private set; }

    public MockList(List<T> items)
    {
        Items = items;
    }
}