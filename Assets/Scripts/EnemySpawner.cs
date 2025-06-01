using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    public GameObject enemyPrefab;
    public GridManager gridManager;

    private List<EnemyController> enemyQueue;

    // Tick-based spawn timing
    private float spawnInterval = 1f;

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
        // Singleton setup
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject); // optional if you want to persist between scenes
    }

    void Start()
    {
        enemyQueue = new List<EnemyController>();
    }

    public void StartNextWave()
    {
        currentWave++;
        enemiesToSpawn = 1 + currentWave; // Wave 1 = 2 enemies, Wave 2 = 3, etc.
        enemiesSpawned = 0;
        isSpawningWave = true;
        textMesh.text = $"{currentWave}";
        Debug.Log($"Starting Wave {currentWave} with {enemiesToSpawn} enemies.");
        GameManager.Instance.AdvanceGame(); // Notify GameManager to advance the game state
    }

    public void SpawnEnemy()
    {
        if (gridManager.pathWorldPositions.Count == 0)
        {
            Debug.LogError("Path not initialized!");
            return;
        }

        GameObject enemy = Instantiate(enemyPrefab);
        EnemyController mover = enemy.GetComponent<EnemyController>();
        mover.SetPath(gridManager.pathWorldPositions);

        float healthMultiplier = 1f + (currentWave - 1) * 0.05f;
        float speedMultiplier = 1f + (currentWave - 1) * 0.05f;
        mover.health = (int) Math.Ceiling(baseHealth * healthMultiplier);
        mover.speed = baseSpeed * speedMultiplier;

        enemyQueue.Add(mover);
    }

    public void RemoveEnemy(EnemyController enemy)
    {
        enemyQueue.Remove(enemy);

        // Auto-start next wave if all enemies are dead and wave was fully spawned
        if (enemyQueue.Count == 0 && !isSpawningWave && currentWave < maxWaves)
        {
            StartNextWave();
        } else if
        (enemyQueue.Count == 0 && currentWave >= maxWaves)
        {
            GameManager.Instance.WinGame(); // Trigger game over if all waves are done
        }
    }


    public void ClearEnemies()
    {
        foreach (var enemy in enemyQueue)
        {
            if (enemy != null)
                Destroy(enemy.gameObject);
        }

        enemyQueue.Clear();
    }

    public void ResetSpawner()
    {
        currentWave = 0;
        enemiesToSpawn = 0;
        enemiesSpawned = 0;
        isSpawningWave = false;
        ClearEnemies();
        textMesh.text = "0";
    }

    public bool finished()
    {
        return currentWave >= maxWaves && enemyQueue.Count == 0 && !isSpawningWave;
    }

    void OnEnable()
    {
        TickSystem.OnTick += HandleTick;
    }

    void OnDisable()
    {
        TickSystem.OnTick -= HandleTick;
    }

    void HandleTick()
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
                isSpawningWave = false; // Done spawning this wave
            }
        }
    }
}
