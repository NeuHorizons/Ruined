using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Define the enum for stat types.
public enum StatType
{
    Damage,
    MovementSpeed,
    Health,
    AttackSpeed,
    Stamina
}

public class StatRowUI : MonoBehaviour
{
    [Header("Cube Settings")]
    [Tooltip("Prefab for one cube. This prefab should be a UI element (with an Image component and RectTransform).")]
    public GameObject cubePrefab;
    [Tooltip("Total number of cubes in the row.")]
    public int maxCubes = 10;

    // List to store references to the instantiated cubes.
    private List<GameObject> cubes = new List<GameObject>();

    [Header("Visual Settings")]
    [Tooltip("Color when the cube is filled (active).")]
    public Color filledColor = Color.green;
    [Tooltip("Color when the cube is empty.")]
    public Color emptyColor = Color.gray;

    [Header("Stat Info")]
    [Tooltip("Select which stat this row represents.")]
    public StatType statType;
    [Tooltip("Reference to the PlayerData ScriptableObject to retrieve the stat level.")]
    public PlayerDataSO playerData;

    [Header("Upgrade Button Settings")]
    [Tooltip("Button at the end of the row that upgrades the stat if enough stat points are available.")]
    public Button upgradeButton;
    [Tooltip("Color for the upgrade button when an upgrade is available (glowing).")]
    public Color upgradeActiveColor = Color.yellow;
    [Tooltip("Color for the upgrade button when upgrade is unavailable (grey).")]
    public Color upgradeInactiveColor = Color.gray;

    private void OnEnable()
    {
        // Only create cubes if we're in play mode.
        if (Application.isPlaying)
        {
            ClearCubes();
            CreateCubes();
            
            // Wire up the upgrade button.
            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeButtonClicked);
            }
        }
        
        // Update the row visuals.
        UpdateUI();
    }

    private void Update()
    {
        // Continuously update the UI in play mode (e.g., if stat points or levels change).
        if (Application.isPlaying)
        {
            UpdateUI();
        }
    }

    private void CreateCubes()
    {
        if (cubePrefab == null)
        {
            Debug.LogWarning("Cube Prefab is not assigned!");
            return;
        }

        cubes = new List<GameObject>();
        for (int i = 0; i < maxCubes; i++)
        {
            GameObject cube = Instantiate(cubePrefab, transform, false);
            cube.name = "Cube_" + i;

            // Set initial color to emptyColor.
            Image img = cube.GetComponent<Image>();
            if (img != null)
            {
                img.color = emptyColor;
            }
            cubes.Add(cube);
        }
    }

    private void ClearCubes()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
        cubes.Clear();
    }

    /// <summary>
    /// Retrieves the stat level from PlayerData based on the stat type and updates the UI.
    /// </summary>
    public void UpdateUI()
    {
        if (playerData == null) return;

        // Determine which stat level to display.
        int statLevel = 0;
        switch (statType)
        {
            case StatType.Damage:
                statLevel = playerData.damageLevel;
                break;
            case StatType.MovementSpeed:
                statLevel = playerData.movementSpeedLevel;
                break;
            case StatType.Health:
                statLevel = playerData.healthLevel;
                break;
            case StatType.AttackSpeed:
                statLevel = playerData.attackSpeedLevel;
                break;
            case StatType.Stamina:
                statLevel = playerData.staminaLevel;
                break;
        }

        UpdateStatLevel(statLevel);
        UpdateUpgradeButton(statLevel);
    }

    /// <summary>
    /// Fills in cubes based on the current stat level (from 0 to maxCubes).
    /// </summary>
    public void UpdateStatLevel(int statLevel)
    {
        statLevel = Mathf.Clamp(statLevel, 0, maxCubes);
        for (int i = 0; i < cubes.Count; i++)
        {
            Image img = cubes[i].GetComponent<Image>();
            if (img != null)
            {
                img.color = (i < statLevel) ? filledColor : emptyColor;
            }
        }
    }

    /// <summary>
    /// Updates the upgrade button’s visuals and interactability.
    /// </summary>
    private void UpdateUpgradeButton(int statLevel)
    {
        if (upgradeButton != null)
        {
            bool canUpgrade = (playerData.statPointsAvailable >= 1) && (statLevel < maxCubes);
            upgradeButton.interactable = canUpgrade;

            Image btnImg = upgradeButton.GetComponent<Image>();
            if (btnImg != null)
            {
                btnImg.color = canUpgrade ? upgradeActiveColor : upgradeInactiveColor;
            }
        }
    }

    /// <summary>
    /// Called when the upgrade button is clicked. 
    /// Upgrades the stat if there are enough stat points and the stat isn’t maxed out.
    /// </summary>
    private void OnUpgradeButtonClicked()
    {
        if (playerData == null) return;

        int statLevel = 0;
        switch (statType)
        {
            case StatType.Damage:
                statLevel = playerData.damageLevel;
                break;
            case StatType.MovementSpeed:
                statLevel = playerData.movementSpeedLevel;
                break;
            case StatType.Health:
                statLevel = playerData.healthLevel;
                break;
            case StatType.AttackSpeed:
                statLevel = playerData.attackSpeedLevel;
                break;
            case StatType.Stamina:
                statLevel = playerData.staminaLevel;
                break;
        }

        if (playerData.statPointsAvailable > 0 && statLevel < maxCubes)
        {
            // Spend one stat point and increment the stat level.
            playerData.statPointsAvailable--;
            switch (statType)
            {
                case StatType.Damage:
                    playerData.damageLevel++;
                    break;
                case StatType.MovementSpeed:
                    playerData.movementSpeedLevel++;
                    break;
                case StatType.Health:
                    playerData.healthLevel++;
                    break;
                case StatType.AttackSpeed:
                    playerData.attackSpeedLevel++;
                    break;
                case StatType.Stamina:
                    playerData.staminaLevel++;
                    break;
            }

            UpdateUI();
        }
    }
}
