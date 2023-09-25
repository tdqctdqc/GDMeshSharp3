
public class Troops : ModelList<Troop>
{
    public Troop Rifle1 { get; private set; }


    public Troops(Items items, TroopTypes troopTypes)
    {
        Rifle1 = new Rifle1(items, troopTypes);
    }
}