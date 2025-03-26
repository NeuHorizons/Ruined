using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    public PlayerDataSO playerData;
    public StatManager statManager;
    public Slider healthSlider;
    private int currentHealth;

    void Start()
    {
        if (playerData == null)
        {
            Debug.LogError("PlayerDataSO is not assigned to PlayerHealth!");
            return;
        }
        if (statManager == null)
        {
            Debug.LogError("StatManager is not assigned to PlayerHealth!");
            return;
        }
        if (healthSlider == null)
        {
            healthSlider = FindObjectOfType<Slider>();
            if (healthSlider == null)
            {
                Debug.LogError("No Health Slider found in the scene!");
                return;
            }
        }
        currentHealth = statManager.FinalHealth;
        healthSlider.maxValue = statManager.FinalHealth;
        healthSlider.value = currentHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, statManager.FinalHealth);
        Debug.Log("Player took " + damage + " damage. Current Health: " + currentHealth);
        UpdateHealthUI();
        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Player has died!");
        EnemyDetection.ResetHiveMind();
        GameManager.Instance.PlayerDied();
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = statManager.FinalHealth;
            healthSlider.value = currentHealth;
        }
        else
        {
            Debug.LogError("Health Slider not assigned in PlayerHealth!");
        }
    }
}