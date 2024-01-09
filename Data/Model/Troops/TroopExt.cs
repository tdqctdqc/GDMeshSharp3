
public static class TroopExt
{
    public static float GetPowerPoints(this Troop t)
    {
        return t.Hardness + t.HardAttack + t.SoftAttack + t.Hitpoints;
    }

    public static float GetAttackPoints(this Troop t)
    {
        return t.HardAttack + t.SoftAttack;
    }
}