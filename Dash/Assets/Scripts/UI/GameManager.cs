using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton for global access
    public GameObject deathScreenUI;    // Assign in Inspector
    public GameObject resistButton;     // Assign UI button
    public GameObject giveUpButton;     // Assign UI button
    public PlayerDataSO playerData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayerDied()
    {
        deathScreenUI.SetActive(true);
        Time.timeScale = 0;
    }
    
    public void GiveUp()
    {
        playerData.currentFloor = 1;
        deathScreenUI.SetActive(false);
        
    }

    public void Resist()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("ResistChallengeScene");
    }
}