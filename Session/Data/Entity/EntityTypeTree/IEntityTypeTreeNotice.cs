using System;
using System.Collections.Generic;
using System.Linq;

public interface IEntityTypeTreeNotice
{
    Entity Entity { get; }
    Type EntityType { get; }
    void HandleForTreeNode(IEntityTypeTreeNode node);
    void Return();
}
