using UnityEngine;

public class BeaconHealth : MonoBehaviour
{
    [Header("Spawner Health Settings")]
    [Tooltip("Current health of the spawner.")]
    public int health = 5;
    
    [Header("Spawner Drop Settings")]
    [Tooltip("Prefab for the drop (e.g., Trader drop) that spawns upon spawner destruction.")]
    public GameObject dropPrefab;
    [Tooltip("Prefab for the hole object.")]
    public GameObject holePrefab;
    [Tooltip("Percentage chance (0 to 1) for the hole to spawn instead of the normal drop.")]
    public float holeSpawnChance = 0.2f; // 20% chance by default.

    // Cache the EnemySpawner component on this object.
    private EnemySpawner enemySpawner;

    private void Awake()
    {
        enemySpawner = GetComponent<EnemySpawner>();
    }
    
    /// <summary>
    /// Updates the spawner's health to the specified value.
    /// </summary>
    /// <param name="newHealth">The new health value for the spawner.</param>
    public void SetHealth(int newHealth)
    {
        health = newHealth;
        Debug.Log("Spawner health updated to: " + health);
    }
    
    /// <summary>
    /// Reduces spawner health by the specified damage amount and checks for destruction.
    /// </summary>
    /// <param name="damage">Amount of damage taken.</param>
    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Spawner took damage! Remaining health: " + health);

        if (health <= 0)
        {
            DestroySpawner();
        }
    }

    /// <summary>
    /// Handles spawner destruction, including dropping an item or a hole based on conditions.
    /// </summary>
    void DestroySpawner()
    {
        Debug.Log("Spawner destroyed! Spawning drop...");
        SpawnDrop();
        
        if (enemySpawner != null)
        {
            enemySpawner.DestroySpawner();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Determines whether to spawn a drop or a hole and instantiates the corresponding prefab.
    /// </summary>
    void SpawnDrop()
    {
        bool spawnHole = false;

        // Check how many EnemySpawner objects remain in the scene.
        // If this is the last spawner, force the hole to spawn.
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        if (spawners.Length == 1)
        {
            spawnHole = true;
            Debug.Log("Last spawner detected. Hole will spawn.");
        }
        else
        {
            // Otherwise, check the chance.
            if (Random.value < holeSpawnChance)
            {
                spawnHole = true;
                Debug.Log("Hole spawn chance met. Hole will spawn.");
            }
        }

        if (spawnHole)
        {
            if (holePrefab != null)
            {
                Debug.Log("Spawning hole: " + holePrefab.name + " at position: " + transform.position);
                Instantiate(holePrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("holePrefab is not assigned! No hole will spawn.");
            }
        }
        else
        {
            if (dropPrefab != null)
            {
                Debug.Log("Spawning drop: " + dropPrefab.name + " at position: " + transform.position);
                Instantiate(dropPrefab, transform.position, Quaternion.identity);
            }
            else
            {
                Debug.LogError("dropPrefab is not assigned! No drop will spawn.");
            }
        }
    }
}
