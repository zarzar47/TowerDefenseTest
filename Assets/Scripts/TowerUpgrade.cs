using UnityEngine;

public class TowerUpgrade
{
    public int Level { get; private set; } = 0;
    public int Cost => Level == 0 ? 1 : 2 * Level;

    public int ApplyUpgrade(int baseDamage)
    {
        Level++;
        float multiplier = Mathf.Pow(1.05f, Level); // 5% increase per level
        return Mathf.CeilToInt(baseDamage * multiplier);
    }
}
