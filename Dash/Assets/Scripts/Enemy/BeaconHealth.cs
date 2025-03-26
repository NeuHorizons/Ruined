using UnityEngine;

public class BeaconHealth : MonoBehaviour
{
    public int health = 5;
    
    // Assign your drop prefab (e.g., Trader drop) in the Inspector.
    public GameObject dropPrefab;
    
    // New: Prefab for the hole object.
    public GameObject holePrefab;
    
    // New: Percentage chance (0 to 1) for the hole to spawn instead of the normal drop.
    public float holeSpawnChance = 0.2f; // 20% chance by default.

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Spawner took damage! Remaining health: " + health);

        if (health <= 0)
        {
            DestroySpawner();
        }
    }

    void DestroySpawner()
    {
        Debug.Log("Spawner destroyed! Spawning drop...");
        SpawnDrop();
        Destroy(gameObject);
    }

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
