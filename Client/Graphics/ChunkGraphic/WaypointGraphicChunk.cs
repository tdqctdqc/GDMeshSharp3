using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ColorFunc = System.Func<Waypoint, Data, (Godot.Color, Godot.Color)>; 

public partial class WaypointGraphicChunk : Node2D, IMapChunkGraphicNode
{
    private MapChunk _chunk;
    public string Name { get; private set; }
    Node2D IMapChunkGraphicNode.Node => this;
    private Dictionary<Waypoint, int> _ids;
    private MultiMeshInstance2D _inner, _outer;
    private MeshInstance2D _links;
    private ColorFunc _getColor; 
    protected bool _stale;
    public WaypointGraphicChunk(Func<Waypoint, Data, (Color inner, Color border)> getColor,
        MapChunk chunk, Data d)
    {
        _chunk = chunk;
        _stale = false;
        _getColor = getColor;
        
        var nav = d.Planet.Nav;
        var chunkWps = nav.Waypoints
            .Values
            .Where(wp => _chunk.Polys.Contains(d.Get<MapPolygon>(wp.AssociatedPolyIds.X)))
            .ToList();

        var mb = new MeshBuilder();
        foreach (var chunkWp in chunkWps)
        {
            var offset = _chunk.RelTo.GetOffsetTo(chunkWp.Pos, d);
            foreach (var nWp in chunkWp.GetNeighboringWaypoints(d))
            {
                var nOffset = _chunk.RelTo.GetOffsetTo(nWp.Pos, d);
                if (chunkWp.Id <= nWp.Id) continue;
                mb.AddLine(offset, nOffset, Colors.Red, 5f);
            }
        }

        _links = mb.GetMeshInstance();
        AddChild(_links);
        
        _ids = new Dictionary<Waypoint, int>();
        _outer = new MultiMeshInstance2D();
        _outer.Multimesh = new MultiMesh();
        var outerQuad = new QuadMesh();
        outerQuad.Size = 12f * Vector2.One;
        _outer.Multimesh.Mesh = outerQuad;
        _outer.Multimesh.UseColors = true;
        AddChild(_outer);
        _outer.Multimesh.InstanceCount = chunkWps.Count();
        
        _inner = new MultiMeshInstance2D();
        _inner.Multimesh = new MultiMesh();
        var innerQuad = new QuadMesh();
        innerQuad.Size = 10f * Vector2.One;
        _inner.Multimesh.Mesh = innerQuad;
        _inner.Multimesh.UseColors = true;
        AddChild(_inner);
        _inner.Multimesh.InstanceCount = chunkWps.Count();
        for (var i = 0; i < chunkWps.Count; i++)
        {
            var wp = chunkWps[i];
            var colors = _getColor(wp, d);
            var offset = _chunk.RelTo.GetOffsetTo(wp.Pos, d);
            _inner.Multimesh.SetInstanceColor(i, colors.Item1);
            _inner.Multimesh.SetInstanceTransform2D(i, new Transform2D(0f, offset));
            _outer.Multimesh.SetInstanceColor(i, colors.Item2);
            _outer.Multimesh.SetInstanceTransform2D(i, new Transform2D(0f, offset));
            _ids.Add(wp, i);
        }
    }

    public void MarkStale()
    {
        _stale = true;
    }
    public void Update(Data d, ConcurrentQueue<Action> queue)
    {
        if (_stale)
        {
            queue.Enqueue(() => Update(d));
            _stale = false;
        }
    }
    private void Update(Data data)
    {
        foreach (var kvp in _ids)
        {
            var wp = kvp.Key;
            var id = kvp.Value;
            var colors = _getColor(wp, data);
            _inner.Multimesh.SetInstanceColor(id, colors.Item1);
            _outer.Multimesh.SetInstanceColor(id, colors.Item2);
        }
    }
    
    private static (Color inner, Color border) GetFrontlineColor(Waypoint wp, Data data)
    {
        var forceBalances = data.Context.WaypointForceBalances;
        var player = data.BaseDomain.PlayerAux
            .LocalPlayer;
        var transparent = (Colors.Transparent, Colors.Transparent);
        if (player.Regime.Empty()) return transparent;
        var alliance = player.Regime.Entity(data).GetAlliance(data);
        var frontlineHash = data.HostLogicData.AllianceAis[alliance].MilitaryAi.FrontHash;
        if (frontlineHash.Contains(wp) == false) return transparent;
        
        if (forceBalances.TryGetValue(wp, out var forceBalance) == false
            || forceBalance.ContainsKey(alliance) == false)
        {
            return transparent;
        }
        else if (forceBalance.GetAllianceWithForceSupremacy(data) == alliance)
        {
            return (Colors.Green, Colors.Black);
        }
        else if (forceBalance.GetAllianceWithForceSuperiority(data) == alliance)
        {
            return (Colors.Yellow, Colors.Black);
        }
        else if (forceBalance.GetControllingAlliances().Contains(alliance))
        {
            return (Colors.Orange, Colors.Black);
        }
        else return (Colors.Red, Colors.Black);
    }
    
    
    private static (Color, Color) GetWaypointTypeColor(Waypoint wp, Data data)
    {
        Color color;
        if (wp is RiverMouthWaypoint)
        {
            color = Colors.DodgerBlue.Darkened(.4f);
        }
        else if (wp is RiverWaypoint)
        {
            color = Colors.DodgerBlue;
        }
        else if (wp is SeaWaypoint)
        {
            color = Colors.DarkBlue;
        }
        else if (wp is InlandWaypoint n)
        {
            var roughness = n.Roughness;
            color = Colors.White.Darkened(roughness);
        }
        else if (wp is CoastWaypoint)
        {
            color = Colors.Green;
        }
        else
        {
            throw new Exception();
        }

        return (color, color);
    }

    public static ChunkGraphicLayer<WaypointGraphicChunk> GetLayer(
        Data d, Client client, GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<WaypointGraphicChunk>("Waypoints", segmenter,
            c => new WaypointGraphicChunk(GetWaypointTypeColor, c, d), d);
        Action markAllStale = () =>
        {
            foreach (var wpChunk in l.ByChunkCoords.Values)
            {
                wpChunk.MarkStale();
            }
        };
        d.Notices.FinishedTurnStartCalc.Subscribe(markAllStale);
        d.BaseDomain.PlayerAux.PlayerChangedRegime
            .Subscribe(a => markAllStale());

        l.AddSetting(
            new TypedSettingsOption<ColorFunc>(
                "Fill",
                new List<ColorFunc> { GetWaypointTypeColor, GetFrontlineColor },
                new List<string> { nameof(GetWaypointTypeColor), nameof(GetFrontlineColor) }
            ),
            (chunk, func) =>
            {
                client.QueuedUpdates.Enqueue(() =>
                {
                    foreach (var wpChunk in l.ByChunkCoords.Values)
                    {
                        wpChunk._getColor = func;
                        wpChunk.Update(d);
                    }
                });
            }
        );
        
        l.AddSetting(new BoolSettingsOption("Show links", false),
            (c, b) =>
            {
                client.QueuedUpdates.Enqueue(() => 
                {
                    c._links.Visible = b;
                });
            });
        
        l.EnforceSettings();
        return l;
    }
}
