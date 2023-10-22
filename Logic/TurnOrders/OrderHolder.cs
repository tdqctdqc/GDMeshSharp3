
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

public class OrderHolder
{
    public ConcurrentDictionary<Player, RegimeTurnOrders> PlayerTurnOrders { get; private set; }
    public ConcurrentDictionary<Regime, Task<RegimeTurnOrders>> AiTurnOrders { get; private set; }

    public OrderHolder()
    {
        PlayerTurnOrders = new ConcurrentDictionary<Player, RegimeTurnOrders>();
        AiTurnOrders = new ConcurrentDictionary<Regime, Task<RegimeTurnOrders>>();
    }

    public void Clear()
    {
        PlayerTurnOrders.Clear();
        AiTurnOrders.Clear();
    }
    public void SubmitPlayerTurnOrders(Player player, RegimeTurnOrders orders, Data data)
    {
        if (orders.Tick != data.BaseDomain.GameClock.Tick) throw new Exception();
        var added = PlayerTurnOrders.TryAdd(player, orders);
        if (added == false) throw new Exception();
    }
    public void CalcAiTurnOrders(Data data)
    {
        var major = data.BaseDomain.GameClock.MajorTurn(data);
        
        GetAiRegimeOrders(r => data.HostLogicData.RegimeAis[r].GetTurnOrders(data), data);
    }
    private async void GetAiRegimeOrders(Func<Regime, RegimeTurnOrders> getOrders, Data data)
    {
        var aiRegimes = data.GetAll<Regime>()
            .Where(r => r.IsPlayerRegime(data) == false);
        await Task.Run(() => data.HostLogicData.Context.Calculate(data));
        
        foreach (var aiRegime in aiRegimes)
        {
            if (AiTurnOrders.ContainsKey(aiRegime) == false)
            {
                var task = Task.Run(() =>
                {
                    return (RegimeTurnOrders) getOrders(aiRegime);
                });
                AiTurnOrders.TryAdd(aiRegime, task);
            }
        }
    }

    
    public bool CheckReadyForFrame(Data data, bool majorTurn)
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

        
        var regimeOrdersReady = allPlayersHaveRegime && allPlayersSubmitted
                                                     && allAisHaveEntry && allAisCompleted;

        return regimeOrdersReady;
    }
}