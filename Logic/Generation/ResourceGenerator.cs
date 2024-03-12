
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class ResourceGenerator : Generator
{
    private GenData _data;
    private GenWriteKey _key;

    public override GenReport Generate(GenWriteKey key)
    {
        _data = key.GenData;
        _key = key;
        var report = new GenReport(GetType().Name);
        GenerateResources();
        MakeSureMajorRegimesHaveResources();
        key.Data.Notices.MadeResources.Invoke();
        return report;
    }

    private void GenerateResources()
    {
        var deposits = new ConcurrentDictionary<NaturalResource, Dictionary<MapPolygon, int>>();
        var resources = _data.Models.GetModels<Item>().Values;
        Parallel.ForEach(resources, r =>
        {
            if (r is NaturalResource n == false) return;
            deposits.TryAdd(n, n.GenerateDeposits(_data));
        });
        foreach (var kvp in deposits)
        {
            var resource = kvp.Key;
            var rDeposits = kvp.Value;
            foreach (var kvp2 in rDeposits) ResourceDeposit.Create(resource, kvp2.Key, kvp2.Value, _key);
        }
    }
    private void MakeSureMajorRegimesHaveResources()
    {
        var majors = _data.GetAll<Regime>().Where(r => r.IsMajor);
        foreach (var regime in majors)
        {
            addResource(regime, _data.Models.Items.Iron);
            addResource(regime, _data.Models.Items.Oil);
            addResource(regime, _data.Models.Items.Coal);
        }

        void addResource(Regime regime, NaturalResource res)
        {
            var has = regime.GetPolys(_key.Data).Any(p =>
            {
                var deps = p.GetResourceDeposits(_data);
                if (deps == null) return false;
                return deps.Any(d => d.Item.Get(_data) == res);
            });
            if (has) return;
            var poly = regime.GetPolys(_key.Data).OrderBy(p => res.GetDepositScore(p, _data)).First();
            var size = res.GenerateDepositSize(poly);
            ResourceDeposit.Create(res, poly, size, _key);
        }
    }
}
