
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public class ItemManager : IModelManager<Item>
{
    public static Food Food { get; private set; } = new Food();
    public static Recruits Recruits { get; private set; } = new Recruits();
    public static Iron Iron { get; private set; } = new Iron();
    public static Oil Oil { get; private set; } = new Oil();
    public static IndustrialPoint IndustrialPoint { get; private set; } = new IndustrialPoint();
    public static FinancialPower FinancialPower { get; private set; } = new FinancialPower();
    public Dictionary<string, Item> Models { get; private set; }
    public ItemManager()
    {
        var models = GetType().GetStaticPropertiesOfType<Item>();
        Models = models.ToDictionary(i => i.Name, i => i);
    }

}
