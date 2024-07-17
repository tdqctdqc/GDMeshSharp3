using System;
using System.Collections.Generic;
using System.Linq;

public class PeepJobList : ModelManager<PeepJob>
{
    public PeepJob Farmer { get; private set; } 
        = new PeepJob();
    public PeepJob Prole { get; private set; } 
        = new PeepJob();
    public PeepJob Miner { get; private set; } 
        = new PeepJob();
    public PeepJob Bureaucrat { get; private set; } 
        = new PeepJob();
    public PeepJob Unemployed { get; private set; } 
        = new PeepJob();
    public PeepJob Herder { get; private set; }
        = new PeepJob();
    public PeepJob Fisher { get; private set; }
        = new PeepJob();
    // public PeepJob Researcher { get; private set; }
    //     = new PeepJob();
}
