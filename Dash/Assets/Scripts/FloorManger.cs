using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance; // Singleton instance

    [Header("Player Data Reference")]
    [Tooltip("Reference to the PlayerData ScriptableObject.")]
    public PlayerDataSO playerData;

    [Header("Floor Tracking")]
    [Tooltip("The player's current floor in the dungeon.")]
    private int currentFloor;
    [Tooltip("The highest floor the player has reached (for leaderboard purposes).")]
    private int highestFloorReached;

    [Header("Level Layout Generator")]
    [Tooltip("Reference to the LevelLayoutGenerator, which builds the dungeon layout for the current floor.")]
    public LevelLayoutGenerator levelLayoutGenerator;

    [Header("Enemy Multipliers")]
    [Tooltip("Multiplier for enemy health per floor. Final health = enemyBaseHealth * (enemyHealthMultiplier)^(floor-1).")]
    public float enemyHealthMultiplier = 1.1f;
    [Tooltip("Multiplier for enemy damage per floor. Final damage = enemyBaseDamage * (enemyDamageMultiplier)^(floor-1).")]
    public float enemyDamageMultiplier = 1.1f;
    [Tooltip("Multiplier for enemy speed per floor. Final speed = enemyBaseSpeed * (enemySpeedMultiplier)^(floor-1).")]
    public float enemySpeedMultiplier = 1.05f;
    [Tooltip("Multiplier for enemy XP per floor. Final XP = baseXP * (enemyXPMultiplier)^(floor-1).")]
    public float enemyXPMultiplier = 1.1f;

    [Header("Spawner Multipliers")]
    [Tooltip("Multiplier for spawn rate per floor. Final spawn rate = spawner's base spawn rate * (spawnRateMultiplier)^(floor-1).")]
    public float spawnRateMultiplier = 0.95f;
    [Tooltip("Multiplier for maximum enemies per floor. Final max enemies = baseMaxEnemies * (maxEnemyMultiplier)^(floor-1).")]
    public float maxEnemyMultiplier = 1.1f;
    [Tooltip("Multiplier for spawner health per floor. Final spawner health = BeaconHealth.baseHealth * (spawnerHealthMultiplier)^(floor-1).")]
    public float spawnerHealthMultiplier = 1.1f;

    private void Awake()
    {
        // Setup singleton instance.
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerDataSO is not assigned to FloorManager!");
            return;
        }

        // Load current floor & highest floor from PlayerData.
        currentFloor = playerData.currentFloor;
        highestFloorReached = playerData.highestFloor;

        if (currentFloor > highestFloorReached)
        {
            highestFloorReached = currentFloor;
            playerData.highestFloor = highestFloorReached;
        }

        UpdatePlayerDataFloorInfo();

        if (levelLayoutGenerator != null)
            levelLayoutGenerator.GenerateLevel();

        AdjustSpawnerDifficulty();
    }

    /// <summary>
    /// Advances to the next floor, updates PlayerData, regenerates the level, and adjusts spawner difficulty.
    /// </summary>
    public void AdvanceToNextFloor()
    {
        currentFloor++;
        if (currentFloor > highestFloorReached)
        {
            highestFloorReached = currentFloor;
            playerData.highestFloor = highestFloorReached;
        }

        UpdatePlayerDataFloorInfo();

        if (levelLayoutGenerator != null)
            levelLayoutGenerator.GenerateLevel();

        AdjustSpawnerDifficulty();
    }

    /// <summary>
    /// Updates PlayerData to reflect the current floor.
    /// </summary>
    private void UpdatePlayerDataFloorInfo()
    {
        playerData.currentFloor = currentFloor;
    }

    /// <summary>
    /// Finds all EnemySpawner objects in the scene and adjusts their stats using multiplicative scaling.
    /// </summary>
    private void AdjustSpawnerDifficulty()
    {
        EnemySpawner[] spawners = FindObjectsOfType<EnemySpawner>();
        if (spawners != null)
        {
            foreach (EnemySpawner spawner in spawners)
            {
                if (spawner != null)
                {
                    // Calculate final spawn rate: baseSpawnInterval * (spawnRateMultiplier)^(currentFloor - 1)
                    float finalSpawnRate = spawner.baseSpawnInterval * Mathf.Pow(spawnRateMultiplier, currentFloor - 1);
                    spawner.SetFinalSpawnInterval(finalSpawnRate);

                    // Calculate final maximum enemy count: baseMaxEnemies * (maxEnemyMultiplier)^(currentFloor - 1)
                    int finalMaxEnemies = Mathf.RoundToInt(spawner.baseMaxEnemies * Mathf.Pow(maxEnemyMultiplier, currentFloor - 1));
                    spawner.SetFinalMaxEnemies(finalMaxEnemies);

                    // Update the spawner's health via BeaconHealth, if available.
                    BeaconHealth beacon = spawner.GetComponent<BeaconHealth>();
                    if (beacon != null)
                    {
                        int finalSpawnerHealth = Mathf.RoundToInt(beacon.health * Mathf.Pow(spawnerHealthMultiplier, currentFloor - 1));
                        beacon.SetHealth(finalSpawnerHealth);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Called when the player enters the "hole" trigger. Advances to the next floor and reloads the scene.
    /// </summary>
    public void EnterNextFloor()
    {
        AdvanceToNextFloor();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Resets the current floor to 1 when returning to the main menu.
    /// </summary>
    public void ResetFloorProgress()
    {
        playerData.currentFloor = 1;
    }

    /// <summary>
    /// Calculates and applies final enemy stats using multipliers.
    /// Final stat = enemy base stat * (multiplier)^(currentFloor - 1).
    /// </summary>
    /// <param name="enemy">The enemy whose final stats will be calculated.</param>
    public void ApplyFinalEnemyStats(Enemy enemy)
    {
        int finalHealth = Mathf.RoundToInt(enemy.enemyBaseHealth * Mathf.Pow(enemyHealthMultiplier, currentFloor - 1));
        int finalDamage = Mathf.RoundToInt(enemy.enemyBaseDamage * Mathf.Pow(enemyDamageMultiplier, currentFloor - 1));
        float finalSpeed = enemy.enemyBaseSpeed * Mathf.Pow(enemySpeedMultiplier, currentFloor - 1);
        int finalXP = Mathf.RoundToInt(enemy.baseXP * Mathf.Pow(enemyXPMultiplier, currentFloor - 1));

        enemy.ApplyFinalStats(finalHealth, finalDamage, finalSpeed);
        enemy.ApplyFinalXP(finalXP);
    }
}
