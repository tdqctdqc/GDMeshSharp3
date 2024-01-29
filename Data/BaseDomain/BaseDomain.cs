using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain
{
    public PlayerAux PlayerAux { get; private set; }
    public GameClock GameClock => _gameClockAux.Value;
    private GameClockAux _gameClockAux;
    public RuleVars Rules => _ruleVarsAux.Value;
    private SingletonAux<RuleVars> _ruleVarsAux;
    public IdDispenser IdDispenser => _idAux.Value;
    private SingletonAux<IdDispenser> _idAux;
    public BaseDomain()
    {
        
    }

    public void Setup(Data data)
    {
        PlayerAux = new PlayerAux(data);
        _gameClockAux = new GameClockAux(data);
        _ruleVarsAux = new SingletonAux<RuleVars>(data);
        _idAux = new SingletonAux<IdDispenser>(data);
    }
}
