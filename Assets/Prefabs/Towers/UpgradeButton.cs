using UnityEngine;

public class UpgradeButton : MonoBehaviour
{
    private Tower Parentower; // Reference to the Tower script
    private GameManager gameManager;
    
    void Start()
    {
        gameManager = GameManager.Instance;
        Parentower = GetComponentInParent<Tower>();
    }

    void OnMouseDown()
    {
        int cost = gameManager.GetMoney();
        int moneySpent = 0;
        if (Parentower.TryUpgrade(cost, out moneySpent))
        {
           gameManager.DisableTowerUpgrades();
           gameManager.SubtractMoney(moneySpent);
           Debug.Log($"Upgrade button clicked. Money spent: {moneySpent}");
           AudioManager.Instance.PlayUpgradeSound();
        }
        
    }

}
