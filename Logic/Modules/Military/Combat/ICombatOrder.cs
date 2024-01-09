using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public interface ICombatOrder 
{
    KeyValuePair<Unit, CombatAction>[] DecideCombatAction(Data d);
}