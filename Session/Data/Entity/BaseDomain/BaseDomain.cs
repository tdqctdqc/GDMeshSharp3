using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain : Domain
{
    public EntityRegister<Player> Players => Data.GetRegister<Player>();
    public PlayerAux PlayerAux { get; private set; }
    public GameClock GameClock => _gameClockAux.Value;
    private GameClockAux _gameClockAux;
    public RuleVars Rules => _ruleVarsAux.Value;
    private SingletonAux<RuleVars> _ruleVarsAux;
    public BaseDomain(Data data) : base(typeof(BaseDomain), data)
    {
        
    }

    public override void Setup()
    {
        PlayerAux = new PlayerAux(Data);
        _gameClockAux = new GameClockAux(Data);
        _ruleVarsAux = new SingletonAux<RuleVars>(Data);
    }
}
