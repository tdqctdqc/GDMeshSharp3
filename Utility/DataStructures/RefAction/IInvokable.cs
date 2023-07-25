using System;
using System.Collections.Generic;
using System.Linq;

public interface IInvokable
{
    void Invoke();
}
public interface IInvokable<T> 
{
    void Invoke(T t);
}
