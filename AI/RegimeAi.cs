
using System.Collections.Generic;
using Godot;

public class RegimeAi
{
    public ERef<Regime> Regime { get; private set; }
    public BudgetAi Budget { get; private set; }
    public RegimeMilitaryAi Military { get; private set; }
    public RegimeTechnologyAi Technology { get; private set; }
    public DiplomacyAi Diplomacy { get; private set; }
    public TimerTreeNode Timer { get; private set; }
    public bool Calculating { get; private set; }

    public static RegimeAi Construct(Regime r, Data d)
    {
        return new RegimeAi(
            r.MakeRef(),
            BudgetAi.Construct(r, d),
            RegimeMilitaryAi.Construct(r, d),
            new RegimeTechnologyAi(r, d),
            new DiplomacyAi(),
            new TimerTreeNode($"{r.Name} Ai"),
            false
        );
    }
    public RegimeAi(ERef<Regime> regime, BudgetAi budget, 
        RegimeMilitaryAi military, RegimeTechnologyAi technology, 
        DiplomacyAi diplomacy,
        TimerTreeNode timer,
         bool calculating)
    {
        Calculating = calculating;
        Regime = regime;
        Budget = budget;
        Military = military;
        Technology = technology;
        Timer = timer;
        Diplomacy = diplomacy;
    }

    public RegimeTurnOrders CalculateAndSendOrders(LogicKey key)
    {
        Calculating = true;
        var major = key.Data.BaseDomain.GameClock.MajorTurn(key.Data);
        RegimeTurnOrders orders = major ? GetMajorTurnOrders(key) : GetMinorTurnOrders(key);
        Calculating = false;
        return orders;
    }
    private MajorTurnOrders GetMajorTurnOrders(LogicKey key)
    {
        Timer = new TimerTreeNode($"{Regime.Get(key.Data).Name} Ai Major");
        Timer.Start();
        var regime = Regime.Get(key.Data);
        var orders = MajorTurnOrders.Construct(key.Data.BaseDomain.GameClock.Tick, regime);
        Diplomacy.Calculate(regime, orders, key);
        Military.CalculateMajor(key, Timer, orders);
        Technology.Calculate(key);
        Budget.Calculate(key, orders);
        // Status.RemoveAt(Status.Count - 1);
        Timer.Stop();
        Game.I.Client.QueuedUpdates.Enqueue(
            () => Game.I.Client.Data.Logger.Log(Timer.GetNode(), LogType.Ai));
        return orders; 
    }
    private MinorTurnOrders GetMinorTurnOrders(LogicKey key)
    {
        Timer = new TimerTreeNode($"{Regime.Get(key.Data).Name} Ai Minor");
        Timer.Start();
        var regime = Regime.Get(key.Data);
        var orders = MinorTurnOrders.Construct(key.Data.BaseDomain.GameClock.Tick, regime);
        
        Military.CalculateMinor(key, Timer, orders);
        Game.I.Client.QueuedUpdates.Enqueue(
            () => Game.I.Client.Data.Logger.Log(Timer.GetNode(), LogType.Ai));

        return orders; 
    }
}
