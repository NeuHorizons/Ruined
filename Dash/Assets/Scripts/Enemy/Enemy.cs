using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Base Stats")]
    [Tooltip("Base enemy health. Used to calculate final health based on floor.")]
    public float enemyBaseHealth = 100f;
    [Tooltip("Base enemy damage. Used to calculate final damage based on floor.")]
    public int enemyBaseDamage = 10;
    [Tooltip("Base enemy speed. Used to calculate final speed based on floor.")]
    public float enemyBaseSpeed = 2f;

    [Header("Final Stats (Calculated based on floor)")]
    [Tooltip("Final enemy health after applying floor modifiers.")]
    public int finalHealth;
    [Tooltip("Final enemy damage after applying floor modifiers.")]
    public int finalDamage;
    [Tooltip("Final enemy speed after applying floor modifiers.")]
    public float finalSpeed;
    [Tooltip("Final XP awarded by this enemy after applying floor modifiers.")]
    public int finalXP;

    [Header("Other Settings")]
    [Tooltip("Base XP awarded by this enemy. Final XP is calculated by FloorManager.")]
    public int baseXP = 10;
    [Tooltip("Reference to the PlayerData ScriptableObject to award XP.")]
    public PlayerDataSO playerData;
    [Tooltip("Soul value awarded on death.")]
    public int soulValue = 5;
    
    // For knockback and movement
    public float knockbackForce = 5f;
    public float knockbackDuration = 0.2f; 
    public float knockbackThreshold = 5f;  
    public float returnDelay = 3f;         
    public float returnSpeed = 2f;         

    private Rigidbody2D rb;
    private Transform player;
    private bool returningToBattle = false;
    private EnemyDetection enemyDetection;
    private EnemySpawner spawner;

    // 'health' holds the current health, set to finalHealth at spawn.
    private int health;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Enemy needs a Rigidbody2D component!");
        }

        enemyDetection = GetComponent<EnemyDetection>();

        // If the enemy is spawned without a spawner (e.g., in a test scene),
        // default final stats to base stats.
        if (spawner == null)
        {
            finalHealth = Mathf.RoundToInt(enemyBaseHealth);
            finalDamage = enemyBaseDamage;
            finalSpeed = enemyBaseSpeed;
            finalXP = baseXP;
        }
        // Now send the base stats to the FloorManager to calculate the final stats.
        if (FloorManager.Instance != null)
        {
            FloorManager.Instance.ApplyFinalEnemyStats(this);
        }

        health = finalHealth;
    }

    private void Update()
    {
        if (!returningToBattle && rb.velocity.magnitude > knockbackThreshold)
        {
            StartCoroutine(ReturnToBattle());
        }
    }

    private IEnumerator ReturnToBattle()
    {
        returningToBattle = true;
        yield return new WaitForSeconds(returnDelay); 

        while (Vector2.Distance(transform.position, player.position) > 2f) 
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * returnSpeed;
            yield return null;
        }

        rb.velocity = Vector2.zero; 
        returningToBattle = false;
    }

    /// <summary>
    /// Called by the FloorManager to apply final stats calculated using the enemy’s base stats.
    /// </summary>
    /// <param name="finalHealth">Final enemy health.</param>
    /// <param name="finalDamage">Final enemy damage.</param>
    /// <param name="finalSpeed">Final enemy speed.</param>
    public void ApplyFinalStats(int finalHealth, int finalDamage, float finalSpeed)
    {
        this.finalHealth = finalHealth;
        this.finalDamage = finalDamage;
        this.finalSpeed = finalSpeed;
        health = finalHealth;
    }

    /// <summary>
    /// Called by the FloorManager to apply the final XP awarded by this enemy.
    /// </summary>
    /// <param name="finalXP">Final XP value.</param>
    public void ApplyFinalXP(int finalXP)
    {
        this.finalXP = finalXP;
    }

    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
    
        if (enemyDetection != null)
        {
            enemyDetection.ActivateChase();
            enemyDetection.AlertNearbyEnemies();
        }
    
        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        SoulManager.Instance.AddSouls(soulValue);

        if (playerData != null)
        {
            // Award XP using the final calculated XP.
            playerData.currentExp += finalXP;
            Debug.Log("Enemy died: Awarded XP: " + finalXP);
        }

        if (spawner != null)
        {
            spawner.EnemyDied();
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            if (playerHealth != null)
            {
                // Use the final calculated damage.
                playerHealth.TakeDamage(finalDamage);
            }

            if (playerRb != null)
            {
                StartCoroutine(ApplyKnockback(playerRb, collision.transform.position));
            }
        }
    }

    private IEnumerator ApplyKnockback(Rigidbody2D playerRb, Vector2 playerPosition)
    {
        Vector2 knockbackDirection = (playerPosition - (Vector2)transform.position).normalized;
        playerRb.velocity = knockbackDirection * knockbackForce;

        yield return new WaitForSeconds(knockbackDuration);
        playerRb.velocity = Vector2.zero; 
    }
}
