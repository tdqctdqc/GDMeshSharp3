
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
        return report;
    }

    private void GenerateResources()
    {
        //todo not fair! will under-generate later resources in list
        var resources = _data.Models
            .GetModels<Item>().Values
            .OfType<NaturalResource>();
        
        foreach (var nr in resources)
        {
            var deposits = nr
                .GenerateDeposits(_data)
                .Where(c => c.HasResourceDeposit(_data) == false);
            foreach (var cell in deposits)
            {
                ResourceDeposit.Create(nr, cell, _key);
            }
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
            var has = regime.GetCells(_data).Any(p =>
            {
                if (p.HasResourceDeposit(_data) == false) return false;
                
                var dep = p.GetResourceDeposit(_data);
                if (dep == null) return false;
                return dep.Item.Get(_data) == res;
            });
            if (has) return;
            var cell = regime.GetCells(_key.Data)
                .OrderBy(p => res.GetDepositScore(p, _data)).First();
            ResourceDeposit.Create(res, cell, _key);
        }
    }
}
