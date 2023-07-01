using System;
using System.Collections.Generic;
using System.Linq;

public interface IEntityRegister
{
    IReadOnlyCollection<Entity> Entities { get; }
    Type EntityType { get; }
}
