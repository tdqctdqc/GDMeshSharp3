
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    public DeploymentAi Deployment { get; private set; }
    public StrategicAi Strategic { get; private set; }
    public AllianceMilitaryAi(Alliance a, Data d)
    {
        Deployment = DeploymentAi.Construct(a, d);
        Strategic = new StrategicAi(d, a);
    }
    public void Calculate(LogicKey key, Alliance alliance)
    {
    }

    public void CalculateMinor(LogicKey key, Alliance alliance)
    {
        Strategic.Calculate(key.Data);
        Deployment.Calculate(this, key);
    }
}