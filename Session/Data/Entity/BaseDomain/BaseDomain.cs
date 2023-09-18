using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain : Domain
{
    public PlayerAux PlayerAux { get; private set; }
    public GameClock GameClock => _gameClockAux.Value;
    private GameClockAux _gameClockAux;
    public RuleVars Rules => _ruleVarsAux.Value;
    private SingletonAux<RuleVars> _ruleVarsAux;
    public IdDispenser IdDispenser => _idAux.Value;
    private SingletonAux<IdDispenser> _idAux;
    public BaseDomain(Data data) : base(typeof(BaseDomain), data)
    {
        
    }

    public override void Setup()
    {
        PlayerAux = new PlayerAux(Data);
        _gameClockAux = new GameClockAux(Data);
        _ruleVarsAux = new SingletonAux<RuleVars>(Data);
        _idAux = new SingletonAux<IdDispenser>(Data);
    }
}
