using UnityEngine;

public class SoulManager : MonoBehaviour
{
    public static SoulManager Instance { get; private set; }
    public PlayerDataSO playerData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddSouls(int amount)
    {
        if (playerData != null)
        {
            playerData.soulCount += amount;
            Debug.Log("Souls collected: " + amount + " | Total Souls: " + playerData.soulCount);
        }
        else
        {
            Debug.LogError("PlayerDataSO is not assigned to SoulManager!");
        }
    }
}