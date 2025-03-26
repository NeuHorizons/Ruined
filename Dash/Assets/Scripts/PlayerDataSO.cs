using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "Game/Player Data")]
public class PlayerDataSO : ScriptableObject
{
    // Base Player Data (non-progression related)
    public int soulCount = 0;
    public bool dashUnlocked = false;
    // New field for the character's name.
    public string characterName = "Default Name";
    
    // Character Progression Data
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float expToNextLevel = 100f;
    public int statPointsAvailable = 0;

    // NEW FIELDS FOR FLOOR MANAGER
    public int currentFloor = 1;
    public int highestFloor = 1;

    // Consolidated Stats: Base values represent points allocated via level-ups.
    [Header("Base Stats (Level-Up Allocated)")]
    public float baseMovementSpeed = 5f;
    public int baseDamage = 10;
    public int baseHealth = 100;
    public float baseAttackSpeed = 0.5f;
    public float baseStamina = 100f;

    // New fields for tracking Natural Progression Points:
    // (These accumulate over time until a threshold is reached to upgrade a stat.)
    [Header("Natural Progression Points")]
    [Tooltip("Accumulated natural progression points for Movement Speed.")]
    public float movementSpeedNaturalPoints = 0f;
    [Tooltip("Accumulated natural progression points for Damage.")]
    public int damageNaturalPoints = 0;
    [Tooltip("Accumulated natural progression points for Health.")]
    public int healthNaturalPoints = 0;
    [Tooltip("Accumulated natural progression points for Attack Speed.")]
    public float attackSpeedNaturalPoints = 0f;
    [Tooltip("Accumulated natural progression points for Stamina.")]
    public float staminaNaturalPoints = 0f;

    // New fields for tracking each stat's level.
    [Header("Stat Levels")]
    [Tooltip("Current level of Movement Speed stat.")]
    public int movementSpeedLevel = 0;
    [Tooltip("Current level of Damage stat.")]
    public int damageLevel = 0;
    [Tooltip("Current level of Health stat.")]
    public int healthLevel = 0;
    [Tooltip("Current level of Attack Speed stat.")]
    public int attackSpeedLevel = 0;
    [Tooltip("Current level of Stamina stat.")]
    public int staminaLevel = 0;

    // Computed properties for overall stat values.
    // (These can be adjusted later if you want to combine base stats with stat levels.)
    public float MovementSpeed { get { return baseMovementSpeed; } }
    public int Damage { get { return baseDamage; } }
    public int Health { get { return baseHealth; } }
    public float AttackSpeed { get { return baseAttackSpeed; } }
    public float Stamina { get { return baseStamina; } }

    /// <summary>
    /// Resets all player data back to its default values.
    /// </summary>
    public void ResetData()
    {
        // Reset base player data
        soulCount = 0;
        dashUnlocked = false;
        characterName = "Default Name";

        // Reset progression data
        currentLevel = 1;
        currentExp = 0f;
        expToNextLevel = 100f;
        statPointsAvailable = 0;

        // Reset floor data
        currentFloor = 1;
        highestFloor = 1;

        // Reset base stats (level-up allocated)
        baseMovementSpeed = 5f;
        baseDamage = 10;
        baseHealth = 100;
        baseAttackSpeed = 0.5f;
        baseStamina = 100f;

        // Reset natural progression points
        movementSpeedNaturalPoints = 0f;
        damageNaturalPoints = 0;
        healthNaturalPoints = 0;
        attackSpeedNaturalPoints = 0f;
        staminaNaturalPoints = 0f;

        // Reset stat levels
        movementSpeedLevel = 0;
        damageLevel = 0;
        healthLevel = 0;
        attackSpeedLevel = 0;
        staminaLevel = 0;
    }
}
