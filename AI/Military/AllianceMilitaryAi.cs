
using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public class AllianceMilitaryAi
{
    public DeploymentAi Deployment { get; private set; }
    public StrategicAi Strategic { get; private set; }
    public static AllianceMilitaryAi Construct(Alliance a, Data d)
    {
        return new AllianceMilitaryAi(DeploymentAi.Construct(a, d),
            StrategicAi.Construct(a, d));
    }

    public AllianceMilitaryAi(DeploymentAi deployment, StrategicAi strategic)
    {
        Deployment = deployment;
        Strategic = strategic;
    }

    public void Calculate(LogicKey key, Alliance alliance)
    {
    }

    public void CalculateMinor(LogicKey key, Alliance alliance)
    {
        Strategic.Calculate(alliance, key);
        Deployment.Calculate(alliance, key);
    }
}