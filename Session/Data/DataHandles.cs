using System;
using System.Collections.Generic;
using System.Linq;

public class DataHandles
{
    public Dictionary<int, Proposal> Proposals { get; private set; } = new ();
    
    //todo wrinkle, index wont be current in client vvv
    public IdDispenser ProposalIds { get; private set; } = new ();
}
