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
    public WaypointGraphicChunk(
        Func<MapPolygon, IEnumerable<Waypoint>> getWps,
        Func<Waypoint, IEnumerable<Waypoint>> getNeighbors, 
        Func<Waypoint, Data, (Color inner, Color border)> getColor,
        MapChunk chunk, Data d)
    {
        _chunk = chunk;
        _stale = false;
        _getColor = getColor;
        
        var chunkWps = chunk.Polys
            .SelectMany(getWps)
            .Distinct()
            .ToList();

        var mb = new MeshBuilder();
        foreach (var chunkWp in chunkWps)
        {
            var offset = _chunk.RelTo.GetOffsetTo(chunkWp.Pos, d);
            foreach (var nWp in getNeighbors(chunkWp))
            {
                var nOffset = _chunk.RelTo.GetOffsetTo(nWp.Pos, d);
                if (chunkWp.Id <= nWp.Id) continue;
                mb.AddLine(offset, nOffset, Colors.Red, 1f);
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
        if (forceBalances.ContainsKey(wp) == false) return (Colors.White, Colors.White);
        var player = data.BaseDomain.PlayerAux
            .LocalPlayer;
        if (player.Regime.Empty()) return (Colors.Transparent, Colors.Transparent);
        var alliance = player.Regime.Entity(data).GetAlliance(data);
        var forceBalance = data.Context.WaypointForceBalances[wp];
        var controlling = forceBalance.GetControllingAlliances();
        var hasAlliance = forceBalance.IsAllianceControlling(alliance);
        if (hasAlliance)
        {
            if (wp.IsDirectlyThreatened(alliance, data))
            {
                var lerp = forceBalance.ByAlliance[alliance]
                           / (2f * forceBalance.GetHostilePowerPoints(alliance, data));
                lerp = Mathf.Clamp(lerp, 0f, 1f);
                var col = Colors.Red.Lerp(Colors.Green, lerp);
                return (col, Colors.Red);
            }
            else if (wp.IsIndirectlyThreatened(alliance, data))
            {
                var lerp = forceBalance.ByAlliance[alliance]
                           / (forceBalance.GetHostilePowerPointsOfNeighbors(wp, alliance, data));
                lerp = Mathf.Clamp(lerp, 0f, 1f);
                var col = Colors.Red.Lerp(Colors.Green, lerp);
                return (col, Colors.Orange);
            }
            return (Colors.White, Colors.Green);
        }
        else
        {
            if (wp.IsDirectlyThreatened(alliance, data)) return (Colors.Black, Colors.Black);
            return (Colors.White, Colors.White);
        }
    }
    
    
    private static (Color, Color) GetWaypointTypeColor(Waypoint wp, Data data)
    {
        Color color;
        if (wp is RiverMouthWaypoint rm)
        {
            color = Colors.DodgerBlue.Darkened(.4f);
            if (rm.Bridgeable) color = Colors.ForestGreen;
        }
        else if (wp is RiverWaypoint r)
        {
            color = Colors.DodgerBlue;
            if (r.Bridgeable) color = Colors.ForestGreen;
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
    
    private static (Color, Color) GetOccupierColor(Waypoint wp, Data data)
    {
        Color color = Colors.Transparent;
        if (data.Military.TacticalWaypoints.OccupierRegimes.TryGetValue(wp.Id, out var rId))
        {
            if (data.Get<Regime>(rId) is Regime r)
            {
                color = r.PrimaryColor;
            }
        }

        return (color, color);
    }
    
    private static (Color, Color) GetResponsibilityColor(Waypoint wp, Data data)
    {
        Color color = Colors.Transparent;

        
        if (data.Context.ControlledAreas
            .Any(kvp => kvp.Value.Contains(wp)))
        {
            var controller = data.Context.ControlledAreas
                .FirstOrDefault(kvp => kvp.Value.Contains(wp));

            if (data.HostLogicData.AllianceAis.Dic
                .TryGetValue(controller.Key, out var ai))
            {
                if (ai
                    .MilitaryAi.AreasOfResponsibility.Any(kvp => kvp.Value.Contains(wp)))
                {
                    var responsible = ai
                        .MilitaryAi.AreasOfResponsibility
                        .FirstOrDefault(kvp => kvp.Value.Contains(wp));
                    color = responsible.Key.PrimaryColor;
                }
                else
                {
                    GD.Print("no responsible at " + wp.AssocPolys(data).First().Id);
                }
            }
        }

        return (color, color);
    }

    public static ChunkGraphicLayer<WaypointGraphicChunk> GetLayer(
        LayerOrder z, string name, Data d, Client client, 
        Func<MapPolygon, IEnumerable<Waypoint>> getWps,
        Func<Waypoint, IEnumerable<Waypoint>> getNeighbors,
        GraphicsSegmenter segmenter)
    {
        var l = new ChunkGraphicLayer<WaypointGraphicChunk>(z, 
            name, 
            segmenter,
            c => 
                new WaypointGraphicChunk(getWps, getNeighbors, GetWaypointTypeColor, c, d), 
            d);
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
                new List<ColorFunc> { GetWaypointTypeColor, GetFrontlineColor, GetOccupierColor, GetResponsibilityColor },
                new List<string> { nameof(GetWaypointTypeColor), nameof(GetFrontlineColor) , nameof(GetOccupierColor), nameof(GetResponsibilityColor)}
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
