using Godot;
using System;
using System.Collections.Generic;

public class BaseDomain
{
    public PlayerAux PlayerAux { get; private set; }
    public GameClock GameClock => _gameClockCache.Value;
    private GameClockCache _gameClockCache;
    public RuleVars Rules => _ruleVarsCache.Value;
    private SingletonCache<RuleVars> _ruleVarsCache;
    public EntityIds IdDispenser => _idCache.Value;
    private SingletonCache<EntityIds> _idCache;
    public BaseDomain()
    {
        
    }

    public void Setup(Data data)
    {
        PlayerAux = new PlayerAux(data);
        _gameClockCache = new GameClockCache(data);
        _ruleVarsCache = new SingletonCache<RuleVars>(data);
        _idCache = new SingletonCache<EntityIds>(data);
    }
}
