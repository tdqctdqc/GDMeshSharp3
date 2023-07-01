
using System.Collections.Generic;
using Godot;

public abstract class Item : IModel
{
    public string Name { get; private set; }
    public Color Color { get; private set; }
    public int Id { get; private set; }
    public Icon Icon { get; } 
    public AttributeHolder<ItemAttribute> Attributes { get; private set; }
    
    protected Item(string name, Color color, 
        params ItemAttribute[] attributes)
    {
        Attributes = new AttributeHolder<ItemAttribute>();
        foreach (var attribute in attributes)
        {
            Attributes.Add(attribute);
        }
        Name = name;
        Color = color;
        Icon = Icon.Create(Name, Icon.AspectRatio._1x1, 50f);
    }
}
