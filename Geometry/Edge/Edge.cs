using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public struct Edge<T>
{
    public T T1 { get; private set; }
    public T T2 { get; private set; }

    public Edge(T t1, T t2, Func<T,T,bool> larger)
    {
        if (t1.Equals(t2)) throw new Exception();
        T1 = larger(t1, t2) ? t1 : t2;
        T2 = T1.Equals(t1) ? t2 : t1;
    }
    public Edge(T t1, T t2, Func<T,float> rank)
    {
        if (t1.Equals(t2)) throw new Exception();
        T1 = rank(t1) > rank(t2) ? t1 : t2;
        T2 = T1.Equals(t1) ? t2 : t1;
    }
}