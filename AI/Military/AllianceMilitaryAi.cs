
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    public DeploymentAi Deployment { get; private set; }
    public OperationalAi Operational { get; private set; }
    public StrategicAi Strategic { get; private set; }
    public AllianceMilitaryAi(Alliance a, Data d)
    {
        Deployment = DeploymentAi.Construct(a, d);
        Operational = new OperationalAi(d, a);
        Strategic = new StrategicAi(d, a);
    }
    public void Calculate(LogicWriteKey key, Alliance alliance)
    {
    }

    public void CalculateMinor(LogicWriteKey key, Alliance alliance)
    {
        Strategic.Calculate();
        Operational.Calculate(this);
        Deployment.Calculate(this, key);
    }
}