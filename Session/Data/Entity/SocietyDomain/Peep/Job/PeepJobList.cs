using System;
using System.Collections.Generic;
using System.Linq;

public class PeepJobList : ModelList<PeepJob>
{
    public PeepJob Farmer { get; private set; } 
        = new PeepJob(nameof(Farmer), .05f);
    public PeepJob Prole { get; private set; } 
        = new PeepJob(nameof(Prole), .1f);
    public PeepJob Miner { get; private set; } 
        = new PeepJob(nameof(Miner), .1f);
    public PeepJob Bureaucrat { get; private set; } 
        = new PeepJob(nameof(Bureaucrat), .5f);
    public PeepJob Builder { get; private set; } 
        = new PeepJob(nameof(Builder), .1f);
    public PeepJob Unemployed { get; private set; } 
        = new PeepJob(nameof(Unemployed), .1f);
    public PeepJob Herder { get; private set; }
        = new PeepJob(nameof(Herder), .1f);

    public PeepJob Fisher { get; private set; }
        = new PeepJob(nameof(Fisher), .1f);
}
