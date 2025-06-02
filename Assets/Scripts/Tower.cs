using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Represents a tower in the game that can attack enemies within its range.
/// The tower has properties such as attack interval, base damage, cost, and upgrade level.
/// It manages a queue of enemies that enter its range and attacks them at specified intervals.
/// The tower can be upgraded to increase its damage output, and it has a UI element to indicate upgrade availability.
/// The tower's attack logic is controlled by an animator, and it can flip its sprite based on the enemy's position.
/// The attack range of the tower is defined by a collider, and it interacts with enemies that enter or exit this range.
/// </summary>

public class Tower : MonoBehaviour
{
    public float attackInterval = 1f;
    public int baseDamage = 1;
    private int currentDamage;
    [SerializeField] private int cost = 0; // Cost of the tower, can be set in the Inspector
    private int upgradeLevel = 0; // Might want to display levels
    private TowerUpgrade upgradeData = new TowerUpgrade();
    public GridPosition gridPosition; // Stores the logical mapping of the tower's position on the grid
    private List<EnemyController> enemyQueue = new List<EnemyController>();
    private float attackTimer = 0f;
    private Animator animator;
    private SpriteRenderer archerSprite;
    [SerializeField] private Transform UILocation; // Default UI location, can be set in the Inspector
    public GameObject upgradeUIPrefab; // Universal Prefab for upgrade icon, can be set in the Inspector
    private GameObject upgradeUI; // Universal Prefab for upgrade icon, can be set in the Inspector
    public bool canUpgrade = false;
    public AudioClip soundClip;
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        archerSprite = GetComponentInChildren<SpriteRenderer>();
        currentDamage = baseDamage;
        if (upgradeUI == null)
        {
            upgradeUI = Instantiate(upgradeUIPrefab, UILocation.position, Quaternion.identity, transform);
        }
        upgradeUI?.SetActive(false); // Hide upgrade icon by default
    }

    void Update()
    {
        if (canUpgrade) // should optimize this using an event from enemySpawner or Advance() in GameManager
        {   // but keep the logic simple for now
            // Show upgrade UI if canUpgrade is true
            upgradeUI?.SetActive(true);
        }

        bool shouldBeAttacking = enemyQueue.Count > 0; // controlling the animation state based on the queue
        // adjusting the attacking state
        if (animator.GetBool("IsAttacking") != shouldBeAttacking)
        {
            animator.SetBool("IsAttacking", shouldBeAttacking);
        }

        if (!shouldBeAttacking)
        {
            attackTimer = 0f; // Reset timer if not attacking
            return;
        }

        EnemyController currentEnemy = enemyQueue[0];

        if (currentEnemy == null || currentEnemy.IsDead())
        {
            enemyQueue.RemoveAt(0);
            return;
        }

        // this part of the code flips the asset depending on the enemy's position
        Vector3 direction = currentEnemy.transform.position - transform.position;
        if (direction.x > 0)
            archerSprite.flipX = false; // Face right
        else
            archerSprite.flipX = true;  // Face left

        attackTimer += Time.deltaTime;


        if (attackTimer >= attackInterval)
        {
            animator.SetTrigger("Shoot"); // Trigger animation
            // FireArrow(currentEnemy.transform.position);
            // 
            attackTimer = 0f;
        }
    }

    public int GetCost()
    {
        return cost;
    }
    public void setGridPosition(Vector2Int gridPos)
    {
        gridPosition = new GridPosition(gridPos.x, gridPos.y);
        // Debug.Log("Tower position set to: " + gridPosition);
    }
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Enemy entered tower range: " + other.name);
        if (other.CompareTag("Enemy"))
        {
            // Debug.Log("Detected enemy: " + other.name);
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null && !enemyQueue.Contains(enemy))
            {
                enemyQueue.Add(enemy);
            }
        }
    }

    // Basic attack function to make attacking universal for different tower types
    public void Attack()
    {
        if (enemyQueue.Count == 0 || enemyQueue[0] == null) return;
        AudioManager.Instance.PlaySFX(soundClip);
        Debug.Log("Attacking enemy: " + enemyQueue[0].name + " with damage: " + currentDamage);
        enemyQueue[0].TakeDamage(currentDamage);
    }

    private void OnTriggerExit(Collider other)
    {
        // Remove the enemy from the queue when it exits the tower's range
        if (other.CompareTag("Enemy"))
        {
            // Debug.Log("Removing enemy from queue: " + other.name);
            EnemyController enemy = other.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemyQueue.Remove(enemy);
            }
        }
    }

    // Attempt upgrade of the tower depending on the player's money
    public bool TryUpgrade(int playerMoney, out int moneySpent)
    {
        moneySpent = 0;
        bool upgraded = false;
        int upgradeCost = upgradeData.Cost;
        if (playerMoney >= upgradeCost)
        {
            currentDamage = upgradeData.ApplyUpgrade(baseDamage);
            moneySpent = upgradeCost;
            upgraded = true;
        }
        return upgraded;
    }

    public void EnableUpgradeMode(bool enable)
    {
        canUpgrade = enable;
        upgradeUI?.SetActive(enable);
    }
}
