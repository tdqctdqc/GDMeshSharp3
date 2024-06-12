using System.Collections.Generic;

public class RefCol<T> where T : IdRef
{
    public HashSet<T> Items { get; private set; }
    
}