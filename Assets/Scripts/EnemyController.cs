using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyController : MonoBehaviour
{
    private List<Vector3> path;
    private int currentIndex = 0;
    public float speed = 2f;
    public int health = 3; // Health of the enemy, can be used for more complex behaviors
    private int maxHealth = 3; // Maximum health for the enemy, can be used for health bar calculations
    [SerializeField] private Image _healthBar; // Reference to the health bar prefab
    public AudioClip deathSound; // Sound to play on death
    public void SetPath(List<Vector3> worldPath)
    {
        maxHealth = health; // Set max health to the initial health value
        path = new List<Vector3>(worldPath);
        transform.position = path[0];
        currentIndex = 1;
        // _healthBar = GetComponentInChildren<Image>(); // Get the health bar component from the child object
    }

    void Update() // this constains movement logic for the enemy
    {
        if (path == null || currentIndex >= path.Count) return;

        Vector3 target = path[currentIndex];
        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.1f)
        {
            currentIndex++;

            if (currentIndex >= path.Count)
            {
                ReachGoal();
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
        UpdateHealthBar(maxHealth, health);

        if (health <= 0)
        {
            GameManager.Instance.IncrementScore(1); // Example method name
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
