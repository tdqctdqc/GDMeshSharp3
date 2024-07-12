
using System.Collections.Generic;
using System.Linq;
using Godot;

public abstract class Item : IModel, IIconed
{
    public string Name { get; private set; }
    public int Id { get; private set; }
    public Icon Icon { get; }
    
    protected Item(string name, 
        params IItemAttribute[] attributes)
    {
        Name = name;
        Icon = Icon.Create(Name, Vector2I.One);
    }
}
