using System.Collections.Generic;

public interface ICombatGraphEdge
{
    ICombatGraphNode Node1 { get; }
    ICombatGraphNode Node2 { get; }
}