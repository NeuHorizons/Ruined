using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // -------- Main Menu UI --------
    [Header("Main Menu UI")]
    public GameObject mainMenuPanel; // Panel for main menu (toggle on/off)

    // -------- Upgrade UI --------
    [Header("Upgrade UI")]
    public GameObject upgradePanel; // Panel for upgrades
    public TextMeshProUGUI soulText;
    public TextMeshProUGUI dashText;
    // New UI elements:
    [Tooltip("Displays the number of available stat points.")]
    public TextMeshProUGUI statPointsText;
    [Tooltip("Displays the player's current level.")]
    public TextMeshProUGUI levelText;
    [Tooltip("Displays the player's character name.")]
    public TextMeshProUGUI characterNameText;

    public Button upgradeButton; // Optionally used to toggle the upgrade menu

    // -------- Player Data & Merchant Proximity --------
    [Tooltip("Reference to the PlayerData ScriptableObject holding player data.")]
    public PlayerDataSO playerData;
    private bool isNearMerchant = false;
    private bool isUpgradeMenuOpen = false;

    void Start()
    {
        // Validate playerData assignment.
        if (playerData == null)
        {
            Debug.LogError("PlayerDataSO is not assigned to UIManager!");
            return;
        }

        // Optionally assign the upgrade menu toggle to a button.
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(ToggleUpgradeMenu);

        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        UpdateUI();
    }

    void Update()
    {
        // Update UI every frame.
        UpdateUI();

        // If near merchant and the player presses "E", toggle the upgrade menu.
        if (isNearMerchant && Input.GetKeyDown(KeyCode.E))
        {
            ToggleUpgradeMenu();
        }
    }

    // -------- Main Menu Methods --------

    public void ToggleMainMenuPanel()
    {
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(!mainMenuPanel.activeSelf);
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
    
    // -------- Death Option Method --------
    public void Death()
    {
        Time.timeScale = 1;
        // You can load the death scene here if needed.
    }

    // -------- Upgrade Methods --------

    void UnlockDash()
    {
        if (playerData.soulCount >= 20)
        {
            playerData.soulCount -= 20;
            playerData.dashUnlocked = true;
            UpdateUI();
        }
    }

    void ToggleUpgradeMenu()
    {
        isUpgradeMenuOpen = !isUpgradeMenuOpen;
        if (upgradePanel != null)
            upgradePanel.SetActive(isUpgradeMenuOpen);

        // Pause the game when the upgrade menu is open.
        Time.timeScale = isUpgradeMenuOpen ? 0 : 1;
    }

    // -------- UI Update Methods --------

    /// <summary>
    /// Updates the UI text elements to reflect current player data.
    /// </summary>
    void UpdateUI()
    {
        if (playerData == null)
            return;

        if (soulText != null)
            soulText.text = $"Souls: {playerData.soulCount}";
        if (dashText != null)
            dashText.text = $"Dash: {(playerData.dashUnlocked ? "Unlocked" : "Locked")}";
        
        // New UI updates.
        if (statPointsText != null)
            statPointsText.text = $"Stat Points: {playerData.statPointsAvailable}";
        if (levelText != null)
            levelText.text = $"Level: {playerData.currentLevel}";
        if (characterNameText != null)
            characterNameText.text = $"Name: {playerData.characterName}"; // Assumes playerData has a field "characterName"
    }

    // -------- External Trigger Methods --------

    public void SetMerchantProximity(bool isNear)
    {
        isNearMerchant = isNear;
    }
}
