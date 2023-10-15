
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

public class OrderHolder
{
    public ConcurrentDictionary<Player, TurnOrders> PlayerTurnOrders { get; private set; }
    public ConcurrentDictionary<Player, TurnOrders> PlayerAllianceOrders { get; private set; }
    public ConcurrentDictionary<Regime, Task<TurnOrders>> AiTurnOrders { get; private set; }
    public ConcurrentDictionary<Alliance, Task<TurnOrders>> AiAllianceTurnOrders { get; private set; }

    public OrderHolder()
    {
        PlayerTurnOrders = new ConcurrentDictionary<Player, TurnOrders>();
        PlayerAllianceOrders = new ConcurrentDictionary<Player, TurnOrders>();
        AiTurnOrders = new ConcurrentDictionary<Regime, Task<TurnOrders>>();
        AiAllianceTurnOrders = new ConcurrentDictionary<Alliance, Task<TurnOrders>>();
    }

    public void Clear()
    {
        PlayerTurnOrders.Clear();
        PlayerAllianceOrders.Clear();
        AiTurnOrders.Clear();
        AiAllianceTurnOrders.Clear();
    }
    public void SubmitPlayerTurnOrders(Player player, TurnOrders orders, Data data)
    {
        if (orders.Tick != data.BaseDomain.GameClock.Tick) throw new Exception();
        var added = PlayerTurnOrders.TryAdd(player, orders);
        if (added == false) throw new Exception();
    }
    public void CalcAiTurnOrders(Data data)
    {
        if (data.BaseDomain.GameClock.MajorTurn(data))
        {
            inner(r => data.HostLogicData.RegimeAis[r].GetMajorTurnOrders(data));
        }
        else
        {
            inner(r => data.HostLogicData.RegimeAis[r].GetMinorTurnOrders(data));
        }
        
        void inner(Func<Regime, TurnOrders> getOrders)
        {
            var aiRegimes = data.GetAll<Regime>()
                .Where(r => r.IsPlayerRegime(data) == false);
            foreach (var aiRegime in aiRegimes)
            {
                if (AiTurnOrders.ContainsKey(aiRegime) == false)
                {
                    var task = Task.Run(() =>
                    {
                        return (TurnOrders) getOrders(aiRegime);
                    });
                    AiTurnOrders.TryAdd(aiRegime, task);
                }
            }
        }
    }
    public bool CheckReadyForFrame(Data data)
    {
        var players = data.GetAll<Player>();
        var aiRegimes = data.GetAll<Regime>().Where(r => r.IsPlayerRegime(data) == false);

        var playerLedAlliances = data.GetAll<Alliance>()
            .Where(a => a.Leader.Entity(data).IsPlayerRegime(data));
        var aiLedAlliances = data.GetAll<Alliance>().Except(playerLedAlliances);
        
        foreach (var kvp in AiTurnOrders)
        {
            if (kvp.Value.IsFaulted)
            {
                throw kvp.Value.Exception;
            }
        }

        var allPlayersHaveRegime = players.All(p => p.Regime.Empty() == false);
        
        var allPlayersSubmitted = players.All(p => PlayerTurnOrders.ContainsKey(p));

        var allAisHaveEntry = aiRegimes.All(p => AiTurnOrders.ContainsKey(p));

        var allAisCompleted = AiTurnOrders.All(kvp => kvp.Value.IsCompleted);

        var allPlayerLedAlliancesSubmitted = playerLedAlliances
            .All(a => PlayerAllianceOrders.ContainsKey(a.Leader.Entity(data).GetPlayer(data)));
        var allAiAlliancesHaveEntry = aiLedAlliances.All(a => AiAllianceTurnOrders.ContainsKey(a));
        var allAiAlliancesCompleted = AiAllianceTurnOrders.All(kvp => kvp.Value.IsCompleted);
        
        return allPlayersHaveRegime && allPlayersSubmitted 
                                    && allAisHaveEntry && allAisCompleted
            // && allPlayerLedAlliancesSubmitted && allAiAlliancesHaveEntry
            // && allAiAlliancesCompleted
            ;
    }
}