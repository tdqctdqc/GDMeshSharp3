using System.Collections.Generic;

public class CombatGraphEdge
{
    public float AttackerProportion { get; set; }
    public float DefenderProportion { get; set; }
    public Dictionary<Troop, float> AttackerLosses { get; private set; }
    public Dictionary<Troop, float> DefenderLosses { get; private set; }

    public CombatGraphEdge()
    {
        AttackerLosses = new Dictionary<Troop, float>();
        DefenderLosses = new Dictionary<Troop, float>();
    }
    
    
}