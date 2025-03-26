using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public float spawnChance;
    }

    public List<EnemySpawnData> enemyList = new List<EnemySpawnData>();
    public float spawnInterval = 5f;
    public int maxEnemies = 5;
    public float spawnRadius = 3f; // Radius around the spawner for random spawning

    private int currentEnemies = 0;
    private bool isDestroyed = false;

    // --- NEW DIFFICULTY SETTINGS ---
    [Header("Difficulty Settings")]
    [Tooltip("The current floor level as set by the FloorManager.")]
    public int currentFloor = 1;
    [Tooltip("Base spawn interval at floor 1.")]
    public float baseSpawnInterval = 5f;
    [Tooltip("Base maximum number of enemies at floor 1.")]
    public int baseMaxEnemies = 5;
    [Tooltip("How much to decrease spawn interval per floor.")]
    public float spawnIntervalDecreasePerFloor = 0.1f;
    [Tooltip("Additional enemies to add every 5 floors.")]
    public int additionalEnemiesPer5Floors = 1;
    [Tooltip("Base enemy health at floor 1.")]
    public float baseEnemyHealth = 100f;
    [Tooltip("Additional enemy health per floor.")]
    public float enemyHealthIncreasePerFloor = 10f;
    // --- END NEW DIFFICULTY SETTINGS ---

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (!isDestroyed)
        {
            if (currentEnemies < maxEnemies)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnEnemy()
    {
        if (enemyList.Count == 0) return;

        GameObject selectedEnemy = GetRandomEnemy();

        if (selectedEnemy != null)
        {
            Vector3 spawnPosition = GetRandomSpawnPoint();
            GameObject enemyObj = Instantiate(selectedEnemy, spawnPosition, Quaternion.identity);
            
            Enemy enemyScript = enemyObj.GetComponent<Enemy>();

            if (enemyScript != null)
            {
                // Use the enemy's own base enemy health to calculate final health.
                // This method will calculate: finalHealth = enemyBaseHealth + (currentFloor * enemyHealthIncreasePerFloor)
                enemyScript.SetFinalHealth(currentFloor, enemyHealthIncreasePerFloor);
                enemyScript.SetSpawner(this);
            }

            currentEnemies++;
        }
    }

    Vector3 GetRandomSpawnPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius; // Get random point within a circle
        Vector3 randomSpawnPosition = new Vector3(transform.position.x + randomOffset.x, transform.position.y + randomOffset.y, transform.position.z);
        return randomSpawnPosition;
    }

    GameObject GetRandomEnemy()
    {
        float totalWeight = 0f;

        foreach (var enemy in enemyList)
        {
            totalWeight += enemy.spawnChance;
        }

        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0f;

        foreach (var enemy in enemyList)
        {
            cumulativeWeight += enemy.spawnChance;
            if (randomValue <= cumulativeWeight)
            {
                return enemy.enemyPrefab;
            }
        }

        return null;
    }

    public void EnemyDied()
    {
        currentEnemies--;
    }

    public void DestroySpawner()
    {
        isDestroyed = true;
        StopAllCoroutines();
        Destroy(gameObject);
    }

    // --- NEW DIFFICULTY MANAGEMENT METHODS ---

    /// <summary>
    /// Updates the spawner difficulty based on the current floor level.
    /// This method recalculates the spawn interval and max enemy count.
    /// It should be called by the FloorManager whenever the floor level changes.
    /// </summary>
    /// <param name="floorLevel">The current floor level.</param>
    public void UpdateDifficulty(int floorLevel)
    {
        currentFloor = floorLevel;
        // Adjust spawn interval: lower intervals (faster spawns) as floor increases,
        // but do not go below a minimum threshold (e.g., 1 second).
        spawnInterval = Mathf.Max(1f, baseSpawnInterval - (floorLevel * spawnIntervalDecreasePerFloor));
        // Increase max enemies based on floor level (example: add additional enemy every 5 floors).
        maxEnemies = baseMaxEnemies + (Mathf.FloorToInt((float)floorLevel / 5f) * additionalEnemiesPer5Floors);
    }

    /// <summary>
    /// Sets the spawner's spawn rate to a new value.
    /// </summary>
    /// <param name="newSpawnRate">The new spawn rate (in seconds between spawns).</param>
    public void SetSpawnRate(float newSpawnRate)
    {
        spawnInterval = newSpawnRate;
    }

    /// <summary>
    /// Sets the spawner's base enemy health to a new value.
    /// </summary>
    /// <param name="newEnemyHealth">The new base enemy health value.</param>
    public void SetEnemyHealth(float newEnemyHealth)
    {
        baseEnemyHealth = newEnemyHealth;
    }
}
