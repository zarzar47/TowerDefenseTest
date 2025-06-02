using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Controls the behavior of enemies in the game.
/// This includes movement along a path, health management, and interactions with the game manager.
/// Enemies follow a path defined by a list of world positions, moving towards the next point in the path at a specified speed.
/// Enemies can take damage, and when their health reaches zero, they are destroyed.
/// Enemies also interact with the game manager to update the score and player lives when they reach the goal.
/// </summary>
public class EnemyController : MonoBehaviour
{
    private List<Vector3> path;
    private int currentIndex = 0;
    public float speed = 2f;
    public int health = 3; // Health of the enemy, can be used for more complex behaviors
    private int maxHealth = 3; // Maximum health for the enemy, can be used for health bar calculations
    [SerializeField] private Image _healthBar; // Reference to the health bar prefab
    public AudioClip deathSound; // Sound to play on death
    private GridPosition gridPosition; // Reference to the grid position of the 

    void Start()
    {
        Vector2Int coords;
        GridManager.Instance.TryGetTileCoordinate(transform.position, out coords);
        gridPosition = new GridPosition(coords.x, coords.y); // Initialize the grid position based on the current position
    }

    public void SetPath(List<Vector3> worldPath)
    {
        maxHealth = health; // Set max health to the initial health value
        path = new List<Vector3>(worldPath);
        transform.position = path[0];
        currentIndex = 1;
    }

    void Update() // this constains movement logic for the enemy
    {
        if (path == null || currentIndex >= path.Count) return;

        Vector3 target = path[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            transform.position = target; // Snap to the target position to avoid floating point errors
        }

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            currentIndex++; // index corresponds to the next point in the path

            if (currentIndex >= path.Count)
            {
                ReachGoal(); // basic logic for reaching the goal
            }
        }
    }

    void ReachGoal()
    {
        // TODO: Reduce player lives or trigger base damage here
        Debug.Log("Enemy reached the base!");
        GameManager.Instance.LoseLife(); // Call the GameManager to handle life loss
        DestroyEnemy();
    }

    public void DestroyEnemy()
    {

        AudioManager.Instance.PlaySFX(deathSound); // Play death sound
        Destroy(gameObject); // Kill the enemy
    }

    private void OnDestroy() //this is to ensure the enemy removes themselves from the spawner when destroyed
    {
        if (EnemySpawner.Instance != null)
        {
            EnemySpawner.Instance.RemoveEnemy(this);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        UpdateHealthBar(maxHealth, health); // Update the health bar UI with the new health value

        if (health <= 0)
        {
            GameManager.Instance.IncrementScore(1); // Increment score by 1 when the enemy is destroyed
            DestroyEnemy();
        }
    }

    private void UpdateHealthBar(float maxhealth, float currentHealth)
    {
        _healthBar.fillAmount = currentHealth / maxhealth; // Update the health bar fill amount
    }

    public bool IsDead()
    {
        return health <= 0;
    }

}
