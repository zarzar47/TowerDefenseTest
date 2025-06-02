using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("Enemy Prefabs & Spawn Probabilities")]
    public List<GameObject> enemyPrefabs;
    [Range(0f, 1f)] public float enemy0Probability = 0.8f; // Enemy at index 0 = 80% (this is mainly for testing purposes)
    public GridManager gridManager; // can remove this beacuse of the singleton pattern, but keeping it for clarity
    private List<EnemyController> enemyQueue;
    // Wave system
    public int currentWave = 0;
    private int enemiesToSpawn = 0;
    private int enemiesSpawned = 0;
    private int maxWaves = 5;
    private bool isSpawningWave = false;
    private float baseHealth = 3f;
    private float baseSpeed = 2f;
    public TextMeshProUGUI textMesh;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        enemyQueue = new List<EnemyController>();
    }

    public void StartNextWave() // This method is called to start the next wave of enemies
    {
        currentWave++; // Increment the wave count, to keep track of the current wave
        enemiesToSpawn = 1 + currentWave;
        enemiesSpawned = 0; 
        isSpawningWave = true; // Set the flag to indicate that we are spawning a new wave of enemies, this helps us in spawning based on the tick system
        textMesh.text = $"{currentWave}";
        GameManager.Instance.AdvanceGame();
    }

    private GameObject ChooseEnemyPrefab() // This method randomly chooses an enemy prefab based on the defined probabilities, nothing inherently important
    {
        float roll = UnityEngine.Random.value;
        if (enemyPrefabs.Count == 0)
        {
            Debug.LogError("No enemy prefabs assigned!");
            return null;
        }
        if (enemyPrefabs.Count == 1 || roll < enemy0Probability)
        {
            return enemyPrefabs[0];
        }
        else
        {
            return enemyPrefabs[1 % enemyPrefabs.Count]; // fallback if more than 2
        }
    }

    public void SpawnEnemy() // IMPORTANT: This method is called to spawn an enemy, it checks if the path is initialized and then spawns an enemy at the start of the path
    {
        if (gridManager.pathWorldPositions.Count == 0)
        {
            Debug.LogError("Path not initialized!");
            return;
        }

        GameObject chosenPrefab = ChooseEnemyPrefab();
        if (chosenPrefab == null) return;

        GameObject enemy = Instantiate(chosenPrefab);
        EnemyController mover = enemy.GetComponent<EnemyController>();
        mover.SetPath(gridManager.pathWorldPositions);

        float healthMultiplier = 1f + (currentWave - 1) * 0.05f;
        float speedMultiplier = 1f + (currentWave - 1) * 0.05f;
        mover.health = (int)Math.Ceiling(baseHealth * healthMultiplier);
        mover.speed = baseSpeed * speedMultiplier;

        enemyQueue.Add(mover);
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        enemyQueue.Remove(enemy);

        if (enemyQueue.Count == 0 && !isSpawningWave && currentWave < maxWaves) // This checks if all enemies are cleared, so we can start the next wave
        {
            StartNextWave();
        }
        else if (enemyQueue.Count == 0 && currentWave >= maxWaves) // This checks if all enemies are cleared and we have reached the max waves
        {
            GameManager.Instance.WinGame();
        }
    }

    public void ClearEnemies() // clean up method
    {
        foreach (var enemy in enemyQueue)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        enemyQueue.Clear();
    }

    public void ResetSpawner() // clean up method
    {
        currentWave = 0;
        enemiesToSpawn = 0;
        enemiesSpawned = 0;
        isSpawningWave = false;
        ClearEnemies();
        textMesh.text = "0";
    }

    public bool finished() // testing method
    {
        return currentWave >= maxWaves && enemyQueue.Count == 0 && !isSpawningWave;
    }

    void OnEnable()
    {
        TickSystem.OnTick += HandleTick;
    }

    void OnDisable() // This is needed to avoid memory leaks, we need to unsubscribe from the tick system when this script is disabled
    {
        TickSystem.OnTick -= HandleTick;
    }

    void HandleTick() // This method is called every tick, it checks if we are spawning a wave and spawns enemies accordingly in Ticks, keeping things simple and efficient
    {
        if (isSpawningWave)
        {
            if (enemiesSpawned < enemiesToSpawn)
            {
                SpawnEnemy();
                enemiesSpawned++;
            }
            else
            {
                isSpawningWave = false;
            }
        }
    }
}
