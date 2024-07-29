
using System.Collections.Generic;
using System.Linq;
using Godot;

public class MigrationModule : LogicModule
{
    public static float MaxMigrationRatio { get; private set; }
        = .1f;
    public override void Calculate(List<RegimeTurnOrders> orders,
        LogicKey key)
    {
        var regimes = key.Data.GetAll<Regime>();
        var proc = MigrationProcedure.Construct();
        var results = regimes
            .AsParallel()
            .SelectMany(r => DoForRegime(r, key))
            .ToArray();
        proc.Results.AddRange(results);
        key.SendMessage(proc);
    }

    private IEnumerable<(int from, int to, float amt)>
        DoForRegime(Regime r, LogicKey key)
    {
        var res = new List<(int from, int to, float amt)>();
        var cells = r.GetCells(key.Data);
        var balances = cells
            .Select(
                c =>
                {
                    var pop = c.GetPeep(key.Data).Size;
                    var balance = pop - c.GetLaborDemand(key.Data);
                    return (c, balance);
                }
            );
        (LandCell cell, float balance)[] surpluses = balances
            .Where(value => value.balance > 0f)
            .OrderByDescending(value => value.Item2)
            .ToArray();
        (LandCell cell, float balance)[] deficits = balances
            .Where(value => value.balance < 0f)
            .OrderBy(value => value.balance)
            .ToArray();
        var surplusIter = 0;
        var deficitIter = 0;
        while (surplusIter < surpluses.Length
               && deficitIter < deficits.Length)
        {
            var surplus = surpluses[surplusIter];
            if (surplus.balance <= 0f)
            {
                surplusIter++;
                continue;
            }
            var deficit = deficits[deficitIter];
            if (deficit.balance >= 0f)
            {
                deficitIter++;
                continue;
            }

            var transfer = Mathf.Min(surplus.balance, 
                surplus.cell.GetPeep(key.Data).Size * MaxMigrationRatio);
            transfer = Mathf.Min(transfer, -deficit.balance);
            res.Add((surplus.cell.Id, deficit.cell.Id, transfer));
            surpluses[surplusIter] = (surplus.cell, surplus.balance - transfer);
            deficits[deficitIter] = (deficit.cell, deficit.balance + transfer);
        }

        return res;
    }
    
}