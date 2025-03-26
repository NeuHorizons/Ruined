using UnityEngine;

public class LevelLayoutGenerator : MonoBehaviour
{
    [Header("Player Data Reference")]
    [Tooltip("Reference to the PlayerData ScriptableObject.")]
    public PlayerDataSO playerData;

    [Header("Tile Generator Reference")]
    [Tooltip("Reference to the TileCaveGenerator already in the scene.")]
    public TileCaveGenerator tileCaveGenerator;

    // Base stats from TileCaveGenerator (used as starting values)
    private int baseFillPercentage;
    private int baseSmoothingIterations;
    private int baseRoomCount;
    private int baseRoomRadius;
    private float baseRoomEdgeNoise;
    private bool baseStatsSet = false;

    [Header("Fill Percentage Settings (Normal Levels)")]
    [Tooltip("Minimum fill percentage for normal levels.")]
    public int fillPercentageMin = 40;
    [Tooltip("Maximum fill percentage for normal levels.")]
    public int fillPercentageMax = 60;

    [Header("Room Count Increment Settings (Normal Levels)")]
    [Tooltip("Additional room/spawner added per every 5 levels above the base.")]
    public int additionalRoomPerFiveLevels = 1;

    #region Boss Floor Adjustment Settings (Every 10th Level)
    [Header("Boss Floor Adjustment Settings (Every 10th Level)")]
    [Tooltip("Override fill percentage for boss floors.")]
    public int bossFillPercentage = 40;
    [Tooltip("Override smoothing iterations for boss floors.")]
    public int bossSmoothingIterations = 3;
    [Tooltip("Override room count for boss floors (e.g., 1 for a single boss room plus the player's room).")]
    public int bossRoomCount = 1;
    [Tooltip("Override room radius for boss floors.")]
    public int bossRoomRadius = 5;
    [Tooltip("Override room edge noise for boss floors.")]
    public float bossRoomEdgeNoise = 0.2f;
    #endregion

    /// <summary>
    /// Reads the current floor from PlayerData and adjusts the TileCaveGenerator's parameters accordingly.
    /// For normal levels, it sets a random fill percentage and increases room count every 5 levels.
    /// For boss levels, it applies the boss override settings.
    /// Then it triggers the dungeon generation.
    /// </summary>
    public void GenerateLevel()
    {
        if (playerData == null)
        {
            Debug.LogWarning("PlayerData reference is not assigned!");
            return;
        }

        // Get the current floor directly from playerData.
        int currentFloor = playerData.currentFloor;
        Debug.Log("Adjusting layout for floor: " + currentFloor);

        if (tileCaveGenerator == null)
        {
            Debug.LogWarning("TileCaveGenerator reference is not assigned!");
            return;
        }

        // Save the base stats only once.
        SaveBaseStats(tileCaveGenerator);

        if (IsBossLevel(currentFloor))
        {
            // Apply boss override settings.
            AdjustBossTileCaveParameters(tileCaveGenerator);
        }
        else
        {
            // For normal levels, adjust fill percentage and room count only.
            AdjustStandardTileCaveParameters(tileCaveGenerator, currentFloor);
        }

        // Generate the dungeon with the modified parameters.
        tileCaveGenerator.GenerateDungeon();
    }

    /// <summary>
    /// Returns true if the current level is considered a boss level (every 10th level).
    /// </summary>
    private bool IsBossLevel(int floor)
    {
        return floor % 10 == 0;
    }

    /// <summary>
    /// Saves the base values from the TileCaveGenerator so adjustments are made relative to the original settings.
    /// This is done only once.
    /// </summary>
    private void SaveBaseStats(TileCaveGenerator tileGenerator)
    {
        if (!baseStatsSet)
        {
            baseFillPercentage = tileGenerator.fillPercentage;
            baseSmoothingIterations = tileGenerator.smoothingIterations;
            baseRoomCount = tileGenerator.roomCount;
            baseRoomRadius = tileGenerator.roomRadius;
            baseRoomEdgeNoise = tileGenerator.roomEdgeNoise;
            baseStatsSet = true;
            Debug.Log("Base stats saved from TileCaveGenerator.");
        }
    }

    /// <summary>
    /// Adjusts parameters for normal (non-boss) levels.
    /// Sets a random fill percentage between the defined min and max,
    /// and increases room count by one extra room (and spawner) for every 5 levels.
    /// The player's room is always spawned.
    /// </summary>
    private void AdjustStandardTileCaveParameters(TileCaveGenerator tileGenerator, int currentFloor)
    {
        // Randomly choose a fill percentage between the minimum and maximum values.
        tileGenerator.fillPercentage = Random.Range(fillPercentageMin, fillPercentageMax + 1);

        // Increase room count: base count plus one extra room per every 5 levels.
        tileGenerator.roomCount = baseRoomCount + ((currentFloor - 1) / 5) * additionalRoomPerFiveLevels;

        Debug.Log("Adjusted standard TileCaveGenerator parameters for floor " + currentFloor);
    }

    /// <summary>
    /// Adjusts parameters for boss levels using the override values.
    /// This can be configured so that a boss level spawns one big room (and one spawner) plus the player's room.
    /// </summary>
    private void AdjustBossTileCaveParameters(TileCaveGenerator tileGenerator)
    {
        tileGenerator.fillPercentage = bossFillPercentage;
        tileGenerator.smoothingIterations = bossSmoothingIterations;
        tileGenerator.roomCount = bossRoomCount;
        tileGenerator.roomRadius = bossRoomRadius;
        tileGenerator.roomEdgeNoise = bossRoomEdgeNoise;

        Debug.Log("Adjusted boss TileCaveGenerator parameters for a boss level.");
    }
}
