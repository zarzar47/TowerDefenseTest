using UnityEngine;

// Helper class for managing tower upgrades
// This class encapsulates the logic for upgrading a tower's level and calculating the cost and damage increase.
// It can be easily extended to include more features like different upgrade paths, visual effects, etc.
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
