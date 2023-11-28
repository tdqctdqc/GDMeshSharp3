
using System;
using System.Collections.Concurrent;
using System.Linq;
using MessagePack;

public class TheaterGraphicLayer : GraphicLayer<TheaterAssignment, TheaterGraphic>
{
    [SerializationConstructor] private TheaterGraphicLayer(int z, Data data, GraphicsSegmenter segmenter) 
        : base(z, "Theater", segmenter,
            (ta, graphic, seg, q) =>
            {
                q.Enqueue(() => graphic.Draw(ta, data));
            })
    {
    }

    private void Draw(Data data)
    {
        foreach (var front in Graphics.Keys.ToList())
        {
            Remove(front, data);
        }
        var theaters = data.HostLogicData.RegimeAis.Dic.Values
            .SelectMany(rAi => rAi.Military.Deployment.ForceAssignments.SelectWhereOfType<TheaterAssignment>());
        
        foreach (var theater in theaters)
        {   
            Add(theater, data);
        }
    }
    protected override TheaterGraphic GetGraphic(TheaterAssignment key, Data d)
    {
        return new TheaterGraphic(key, _segmenter, d);
    }

    public static TheaterGraphicLayer GetLayer(int z, 
        GraphicsSegmenter seg,
        Client client)
    {
        var l = new TheaterGraphicLayer(z, client.Data, seg);
        
        
        client.Data.Notices.Ticked.Blank.Subscribe(() =>
            {
                client.QueuedUpdates.Enqueue(() =>
                {
                    l.Draw(client.Data);
                    l.EnforceSettings();
                });
            }
        );
        return l;
    }
}