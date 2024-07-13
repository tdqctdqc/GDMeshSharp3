using Godot;

namespace GDMeshSharp3.Session.Data.Model.RoadModels;

public class StoneRoad : RoadModel
{
    public StoneRoad() 
    {
    }

    public override void Draw(MeshBuilder mb, Vector2 from, Vector2 to, float width)
    {
        var stoneColor = Colors.Black.Lightened(.3f);
        var stoneWidth = width / 6f;
        mb.AddLine(from, to, Colors.Black.Lightened(.5f), width);
        var perp = (to - from).Orthogonal().Normalized();
        
        mb.AddDashedLine(from, to, stoneColor, 
            stoneWidth, 
            width / 2f, 
            width / 3f);
        mb.AddDashedLine(from + perp * stoneWidth * 2f, to + perp * stoneWidth * 2f, stoneColor, 
            stoneWidth, 
            width / 2f, 
            width / 3f);
        
        mb.AddDashedLine(from - perp * stoneWidth * 2f, to - perp * stoneWidth * 2f, stoneColor, 
            stoneWidth, 
            width / 2f, 
            width / 3f);
    }
}