using System;
using System.Collections.Generic;
using System.Linq;

public interface IDataSession : ISession
{
    Data Data { get; }
}
