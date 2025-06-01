using TMPro;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using UnityEngine.UIElements;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Gameplay Settings")]
    public int startingLives = 3;
    private int currentLives;
    GridManager gridManager;
    EnemySpawner enemySpawner;
    private List<Tower> towerList;
    public bool IsGameOver { get; private set; } = false;
    public TextMeshProUGUI Lives_text;
    public TextMeshProUGUI Score_text;
    public TextMeshProUGUI Final_Score_text_Victory;
    public TextMeshProUGUI Final_Score_text_Defeat;
    public TextMeshProUGUI Money_text;
    public int enemyKilled = 0;
    public int startingMoney = 5; // Initial money for the player
    private int currentMoney = 0; // This can be used to track player's money
    public GameObject Victory_Panel;
    public GameObject Defeat_Panel;
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        towerList = new List<Tower>();
        Instance = this;
        // DontDestroyOnLoad(gameObject);
        currentLives = startingLives;
        gridManager = FindFirstObjectByType<GridManager>();
        enemySpawner = FindFirstObjectByType<EnemySpawner>();
    }

    void Start()
    {
        currentMoney = startingMoney; // Initialize current money
        Victory_Panel.SetActive(false);
        Defeat_Panel.SetActive(false);
        Lives_text.text = $"{currentLives}";
        Score_text.text = $"{0}";
        Money_text.text = $"{currentMoney}";
        AdvanceGame();
    }

    public void LoseLife(int amount = 1)
    {
        if (IsGameOver) return;

        currentLives -= amount;
        Lives_text.text = $"{currentLives}";
        SubtractMoney(1);

        if (currentLives <= 0)
        {
            GameOver();
        }
    }

    public void GameOver()
    {
        IsGameOver = true;
        Debug.Log("You Lost!");
        Final_Score_text_Defeat.text = $"Final Score: {enemyKilled}";
        Defeat_Panel.SetActive(true);
    }

    public void RestartGame()
    {
        IsGameOver = false;
        currentLives = startingLives;
        currentMoney = 0;
        EnemySpawner.Instance.ResetSpawner();
        foreach (Tower tower in towerList)
        {
            Destroy(tower.gameObject);
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public int GetMoney()
    {
        return currentMoney;
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        if (Money_text != null)
        {
            Money_text.text = $"{currentMoney}";
        }
        else
        {
            Debug.LogWarning("Money TextMeshProUGUI reference is not set in GameManager.");
        }
    }

    public void SubtractMoney(int amount)
    {
        currentMoney -= amount;
        if (currentMoney < 0) currentMoney = 0; // Prevent negative money
        if (Money_text != null)
        {
            Money_text.text = $"{currentMoney}";
        }
        else
        {
            Debug.LogWarning("Money TextMeshProUGUI reference is not set in GameManager.");
        }
    }

    public void AdvanceGame()
    {
        Debug.Log("Advancing game...");

        if (EnemySpawner.Instance.currentWave == 0)
        {
            Debug.Log("Starting first wave...");
            enemySpawner.StartNextWave();
            return;
        }
        Debug.Log($"Current Wave: {EnemySpawner.Instance.currentWave}");
        AddMoney(2); // Add money for advancing the game
        EnableTowerUpgrades();
    }

    public void EnableTowerUpgrades()
    {
        foreach (Tower tower in towerList)
        {
            tower.EnableUpgradeMode(true);
        }
    }

    public void DisableTowerUpgrades()
    {
        foreach (Tower tower in towerList)
        {
            tower.EnableUpgradeMode(false);
        }
    }

    public void PlaceTowerAt(Vector2Int gridPos, GameObject towerPrefab)
    {
        if (gridManager == null) gridManager = FindFirstObjectByType<GridManager>();
        Tile tile = gridManager.GetTile(gridPos.x, gridPos.y);

        if (tile != null && tile.type == TileType.Empty && !tile.isOccupied)
        {
            Vector3 worldPos = tile.visual.transform.position + Vector3.up * 0.5f;
            GameObject towerObj = Instantiate(towerPrefab, worldPos, Quaternion.identity);
            Tower tower = towerObj.GetComponent<Tower>();
            tower.setGridPosition(gridPos);
            towerList.Add(tower); // adds a tower to the list
            SubtractMoney(tower.GetCost()); // Subtract the cost of the tower from current money
            tile.isOccupied = true;
            tile.type = TileType.Blocked;
        }
        else
        {
            Debug.Log("Invalid or occupied tile");
        }
    }

    public void RemoveTower(Tower tower)
    {
        if (towerList.Contains(tower))
        {
            towerList.Remove(tower);
            Tile tile = gridManager.GetTile(tower.gridPosition.x, tower.gridPosition.y);
            if (tile != null)
            {
                tile.isOccupied = false;
                tile.type = TileType.Empty; // Reset tile type
            }
            Destroy(tower.gameObject);
        }
        else
        {
            Debug.LogWarning("Tower not found in the list.");
        }
    }

    public void IncrementScore(int val)
    {
        enemyKilled += val;
        if (Score_text != null)
        {
            Score_text.text = $"{enemyKilled}";
        }
        else
        {
            Debug.LogWarning("Score TextMeshProUGUI reference is not set in GameManager.");
        }
    }

    public void WinGame()
    {
        IsGameOver = true;
        Debug.Log("You Win!");
        Final_Score_text_Victory.text = $"Final Score: {enemyKilled}";
        Victory_Panel.SetActive(true);
    }
}
