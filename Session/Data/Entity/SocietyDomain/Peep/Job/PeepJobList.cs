using System;
using System.Collections.Generic;
using System.Linq;

public class PeepJobList : ModelList<PeepJob>
{
    public PeepJob Farmer { get; private set; } 
        = new PeepJob(nameof(Farmer));
    public PeepJob Prole { get; private set; } 
        = new PeepJob(nameof(Prole));
    public PeepJob Miner { get; private set; } 
        = new PeepJob(nameof(Miner));
    public PeepJob Bureaucrat { get; private set; } 
        = new PeepJob(nameof(Bureaucrat));
    public PeepJob Builder { get; private set; } 
        = new PeepJob(nameof(Builder));
    public PeepJob Unemployed { get; private set; } 
        = new PeepJob(nameof(Unemployed));
    public PeepJob Herder { get; private set; }
        = new PeepJob(nameof(Herder));

    public PeepJob Fisher { get; private set; }
        = new PeepJob(nameof(Fisher));
}
