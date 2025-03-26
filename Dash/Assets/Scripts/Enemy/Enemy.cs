using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Health Settings")]
    [Tooltip("Base enemy health for this enemy. This value is used to calculate the final health based on the current floor.")]
    public float enemyBaseHealth = 3f;
    
    private int health;

    public int soulValue = 5;
    public int damageToPlayer = 10; 

    [Header("XP Settings")]
    [Tooltip("Base XP awarded by this enemy. The final XP reward is proportional to the current floor.")]
    public int baseXP = 10;
    [Tooltip("Reference to the PlayerData ScriptableObject to award XP.")]
    public PlayerDataSO playerData;

    private EnemySpawner spawner;
    private Transform player;

    public float knockbackForce = 5f;    
    public float knockbackDuration = 0.2f; 

    public float knockbackThreshold = 5f;  
    public float returnDelay = 3f;         
    public float returnSpeed = 2f;         

    private Rigidbody2D rb;
    private bool returningToBattle = false;

    
    private EnemyDetection enemyDetection;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Enemy needs a Rigidbody2D component!");
        }

        
        enemyDetection = GetComponent<EnemyDetection>();

        
        if (spawner == null)
        {
            health = Mathf.RoundToInt(enemyBaseHealth);
        }
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
            
            int xpReward = baseXP * playerData.currentFloor;
            playerData.currentExp += xpReward;
            Debug.Log("Enemy died: Awarded XP: " + xpReward);
        }

        
        if (spawner != null)
        {
            spawner.EnemyDied();
        }

        Destroy(gameObject);
    }

    public void SetSpawner(EnemySpawner enemySpawner)
    {
        spawner = enemySpawner;
    }

    
    public void SetFinalHealth(int floor, float enemyHealthIncreasePerFloor)
    {
        health = Mathf.RoundToInt(enemyBaseHealth + (floor * enemyHealthIncreasePerFloor));
    }

    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();
            Rigidbody2D playerRb = collision.GetComponent<Rigidbody2D>();

            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageToPlayer);
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
