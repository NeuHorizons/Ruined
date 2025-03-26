using UnityEngine;
using UnityEngine.SceneManagement;

public class FloorManager : MonoBehaviour
{
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

    [Header("Enemy Difficulty Settings")]
    [Tooltip("Enemy spawner components that will have their spawn rates and enemy stats adjusted based on the floor.")]
    public EnemySpawner[] enemySpawners;
    [Tooltip("Base spawn rate (seconds between spawns) at floor 1.")]
    public float baseSpawnRate = 5f;
    [Tooltip("Amount to decrease the spawn rate per floor (spawn rate gets lower as floor increases).")]
    public float spawnRateDecreasePerFloor = 0.1f;
    [Tooltip("Base enemy health at floor 1.")]
    public float baseEnemyHealth = 100f;
    [Tooltip("Amount to increase enemy health per floor.")]
    public float enemyHealthIncreasePerFloor = 10f;

    private void Start()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerDataSO is not assigned to FloorManager!");
            return;
        }

        // Load current floor & highest floor from PlayerData
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

        AdjustEnemyDifficulty();
    }

    /// <summary>
    /// Advances to the next floor, updates PlayerData, regenerates the level, and adjusts enemy difficulty.
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

        AdjustEnemyDifficulty();
    }

    /// <summary>
    /// Updates PlayerData to reflect the current floor and highest floor reached.
    /// </summary>
    private void UpdatePlayerDataFloorInfo()
    {
        playerData.currentFloor = currentFloor;
    }

    /// <summary>
    /// Adjusts enemy difficulty based on the current floor.
    /// </summary>
    private void AdjustEnemyDifficulty()
    {
        float newSpawnRate = Mathf.Max(1f, baseSpawnRate - (currentFloor * spawnRateDecreasePerFloor));
        float newEnemyHealth = baseEnemyHealth + (currentFloor * enemyHealthIncreasePerFloor);

        if (enemySpawners != null)
        {
            foreach (EnemySpawner spawner in enemySpawners)
            {
                if (spawner != null)
                {
                    spawner.UpdateDifficulty(currentFloor);
                    spawner.SetSpawnRate(newSpawnRate);
                    spawner.SetEnemyHealth(newEnemyHealth);
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
}
