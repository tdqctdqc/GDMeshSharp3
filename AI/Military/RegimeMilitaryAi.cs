
using System.Collections.Generic;
using System.Linq;
using Godot;

public class RegimeMilitaryAi
{
    private ERef<Regime> _regime;
    public DeploymentAi Deployment { get; private set; }
    public StrategicAi Strategic { get; private set; }
    public ForceCompositionAi ForceComposition { get; private set; }
    public UnitTemplatesAi Templates { get; private set; }


    public RegimeMilitaryAi(ERef<Regime> regime, 
        ForceCompositionAi forceComposition, 
        UnitTemplatesAi templates,
        DeploymentAi deployment,
        StrategicAi strategic)
    {
        Strategic = strategic;
        Deployment = deployment;
        _regime = regime;
        ForceComposition = forceComposition;
        Templates = templates;
    }

    public static RegimeMilitaryAi Construct(Regime regime, Data d)
    {
        return new RegimeMilitaryAi(regime.MakeRef(),
            new ForceCompositionAi(new Dictionary<UnitTemplatesAi.UnitTypeTag, int>()),
        UnitTemplatesAi.Construct(regime, d),
            DeploymentAi.Construct(regime, d), 
            StrategicAi.Construct(regime, d));
    }
    public void CalculateMajor(LogicKey key, 
        TimerTreeNode parentTimer,
        MajorTurnOrders orders)
    {
        var timer = parentTimer.AddChildAndStart("Military Major");
        timer.Start();
        ForceComposition.Calculate(_regime.Get(key.Data),
            timer, key);
        Templates.Calculate(timer, key);
        timer.Stop();
    }

    public void CalculateMinor(LogicKey key, 
        TimerTreeNode parentTimer,
        MinorTurnOrders orders)
    {
        var timer = parentTimer.AddChildAndStart("Military Minor");
        timer.Start();
        Strategic.Calculate(_regime.Get(key.Data), timer, key);
        Deployment.Calculate(_regime.Get(key.Data),timer,  key);
        timer.Stop();
    }
    
    
}