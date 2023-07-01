using System;
using System.Collections.Generic;
using System.Linq;

public interface IEntityNotice
{
    Type EntityType { get; }
    void Clear();
}
