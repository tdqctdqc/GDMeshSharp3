
using System.Collections.Generic;
using System.Linq;

public class TroopType : IModel
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    IReadOnlyList<IModelAttribute> IModel.AttributeList => Attributes;
    public AttributeHolder<ITroopAttribute> Attributes { get; }
    public TroopDomain Domain { get; private set; }
    public TroopType(string name, TroopDomain domain)
    {
        Name = name;
        Domain = domain;
        Attributes = new AttributeHolder<ITroopAttribute>();
    }

    
}