
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Item : IModel
{
    public string Name { get; private set; }
    public Color Color { get; private set; }
    public int Id { get; private set; }
    public Icon Icon { get; }
    
    protected Item(string name, Color color, 
        params IItemAttribute[] attributes)
    {
        Name = name;
        Color = color;
        Icon = Icon.Create(Name, Vector2I.One);
    }
}
