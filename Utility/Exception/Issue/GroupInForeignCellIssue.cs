
using Godot;

public class GroupInForeignCellIssue : Issue
{
    private Cell _cell;
    private Army _group;
    public GroupInForeignCellIssue(Army g, Cell cell, int tick) 
        : base(cell.GetCenter(), "", tick)
    {
        _cell = cell;
        _group = g;
        AddLayer("base", mb =>
        {
            var groupP = Pos.Offset(_group.GetHomeCell(Game.I.Client.Data).GetCenter(), Game.I.Client.Data);
            var cellP = Pos.Offset(_cell.GetCenter(), Game.I.Client.Data);
            mb.AddSquare(cellP, 20f, Colors.Black);
            mb.AddSquare(groupP, 10f, _group.Color);
        });
    }
}