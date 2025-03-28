using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    // ---- Enemy Spawn Data ----
    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public float spawnChance;
    }

    public List<EnemySpawnData> enemyList = new List<EnemySpawnData>();

    // ---- Spawn Settings (Non-modifiable) ----
    [Tooltip("Radius around the spawner for random spawning.")]
    public float spawnRadius = 3f;

    private int currentEnemies = 0;
    private bool isDestroyed = false;

    // ---- Base Stats (Used for Spawning) ----
    [Header("Base Stats")]
    [Tooltip("The current floor level as set by the FloorManager.")]
    public int currentFloor = 1;
    [Tooltip("Spawn interval (in seconds) at floor 1.")]
    public float baseSpawnInterval = 5f;
    [Tooltip("Maximum number of enemies at floor 1.")]
    public int baseMaxEnemies = 5;

    // ---- Final Stats (Calculated via FloorManager) ----
    [Header("Final Stats (Calculated)")]
    [Tooltip("Final spawn interval after applying multipliers.")]
    public float finalSpawnInterval;
    [Tooltip("Final maximum number of enemies after applying multipliers.")]
    public int finalMaxEnemies;

    private void Awake()
    {
        // Initialize final stats to base values by default.
        finalSpawnInterval = baseSpawnInterval;
        finalMaxEnemies = baseMaxEnemies;
    }

    private void Start()
    {
        StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        while (!isDestroyed)
        {
            // Use the final maximum enemy count.
            if (currentEnemies < finalMaxEnemies)
            {
                SpawnEnemy();
            }
            yield return new WaitForSeconds(finalSpawnInterval);
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
                // The enemy will request its final stats from FloorManager later.
                enemyScript.SetSpawner(this);
            }

            currentEnemies++;
        }
    }

    Vector3 GetRandomSpawnPoint()
    {
        Vector2 randomOffset = Random.insideUnitCircle * spawnRadius;
        return new Vector3(transform.position.x + randomOffset.x, transform.position.y + randomOffset.y, transform.position.z);
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

    /// <summary>
    /// Called by FloorManager to update the final spawn interval.
    /// </summary>
    /// <param name="newSpawnInterval">Final spawn interval in seconds.</param>
    public void SetFinalSpawnInterval(float newSpawnInterval)
    {
        finalSpawnInterval = newSpawnInterval;
    }

    /// <summary>
    /// Called by FloorManager to update the final maximum enemy count.
    /// </summary>
    /// <param name="newMaxEnemies">Final maximum enemy count.</param>
    public void SetFinalMaxEnemies(int newMaxEnemies)
    {
        finalMaxEnemies = newMaxEnemies;
    }
}
