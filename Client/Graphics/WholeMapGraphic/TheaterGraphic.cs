
using System.Linq;
using Godot;

public partial class TheaterGraphic : Node2D
{
    public TheaterGraphic(TheaterAssignment theater, 
        GraphicsSegmenter segmenter, Data d)
    {
        Draw(theater, d);
        var relToId = theater.TacWaypoints.First();
        var relTo = d.Military.TacticalWaypoints.Waypoints[relToId].Pos;
        segmenter.AddElement(this, relTo);
    }

    public void  Draw(TheaterAssignment theater, Data d)
    {
        this.ClearChildren();
        if (theater.TacWaypoints.Count() == 0) return;
        var alliance = theater.Regime.Entity(d).GetAlliance(d);
        var outer = theater.Regime.Entity(d).PrimaryColor;
        var inner = ColorsExt.GetRandomColor();
        
        var relToId = theater.TacWaypoints.First();
        var relTo = d.Military.TacticalWaypoints.Waypoints[relToId].Pos;
        var mb = new MeshBuilder();
        foreach (var tId in theater.TacWaypoints)
        {
            var wp = d.Military.TacticalWaypoints.Waypoints[tId];
            var relPos = d.Planet.GetOffsetTo(relTo, wp.Pos);
            
            foreach (var nWp in wp.TacNeighbors(d))
            {
                if (theater.TacWaypoints.Contains(nWp.Id) == false) continue;
                if (nWp.Id > tId) continue;
                var nRelPos = d.Planet.GetOffsetTo(relTo, nWp.Pos);

                mb.AddLine(relPos, nRelPos, outer, 2f);
            }
        }
        foreach (var tId in theater.TacWaypoints)
        {
            var wp = d.Military.TacticalWaypoints.Waypoints[tId];
            var relPos = d.Planet.GetOffsetTo(relTo, wp.Pos);
            mb.AddSquare(relPos, 10f, outer);
            mb.AddSquare(relPos, 6f, inner);
        }

        var fronts = theater.Fronts;
        foreach (var fa in fronts)
        {

            var line = fa.Front.GetContactLineWaypoints(d);
            var poses = fa.CalcPoses(line,
                alliance, d);
            
            var frontColor = ColorsExt.GetRandomColor();
            
            
            
            for (var i = 0; i < line.Count - 1; i++)
            {
                var from = d.Planet.GetOffsetTo(relTo, poses[line[i]]);
                var to = d.Planet.GetOffsetTo(relTo, poses[line[i + 1]]);
                mb.AddLine(from, to, outer.Darkened(.25f), 15f);
                // mb.AddLine(from, to, frontColor, 5f);
            }
        }
        
        if (mb.Tris.Count > 0)
        {
            AddChild(mb.GetMeshInstance());   
        }
    }
    
}