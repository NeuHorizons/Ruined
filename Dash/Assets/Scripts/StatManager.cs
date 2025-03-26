using UnityEngine;
using System;

public class StatManager : MonoBehaviour
{
    [Header("Player Data Reference")]
    public PlayerDataSO playerData;
    public event Action<int> OnLevelUp;

    [Header("Stat Multipliers")]
    public float damageMultiplier = 0.1f;
    public float movementSpeedMultiplier = 0.1f;
    public float healthMultiplier = 0.1f;
    public float attackSpeedMultiplier = 0.1f;
    public float staminaMultiplier = 0.1f;

    public int FinalDamage { get { return Mathf.RoundToInt(playerData.baseDamage * (1 + playerData.damageLevel * damageMultiplier)); } }
    public float FinalMovementSpeed { get { return playerData.baseMovementSpeed * (1 + playerData.movementSpeedLevel * movementSpeedMultiplier); } }
    public int FinalHealth { get { return Mathf.RoundToInt(playerData.baseHealth * (1 + playerData.healthLevel * healthMultiplier)); } }
    public float FinalAttackSpeed { get { return playerData.baseAttackSpeed / (1 + playerData.attackSpeedLevel * attackSpeedMultiplier); } }
    public float FinalStamina { get { return playerData.baseStamina * (1 + playerData.staminaLevel * staminaMultiplier); } }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
            AddExperience(2000);
        if (Input.GetKeyDown(KeyCode.R))
            AddDamageDealt(50);

        while (playerData.currentExp >= playerData.expToNextLevel && playerData.currentLevel < 100)
        {
            playerData.currentExp -= playerData.expToNextLevel;
            LevelUp();
        }
    }

    public void AddExperience(float expAmount)
    {
        playerData.currentExp += expAmount;
    }

    void LevelUp()
    {
        playerData.currentLevel++;
        playerData.statPointsAvailable += 1;
        playerData.expToNextLevel *= 1.1f;
        OnLevelUp?.Invoke(playerData.currentLevel);
        Debug.Log("Leveled Up! New level: " + playerData.currentLevel);
    }

    public void AllocateStatPoints(int movementSpeedPoints, int damagePoints, int healthPoints, int attackSpeedPoints, int staminaPoints)
    {
        int totalPoints = movementSpeedPoints + damagePoints + healthPoints + attackSpeedPoints + staminaPoints;
        if (totalPoints > playerData.statPointsAvailable)
        {
            Debug.LogWarning("Not enough stat points available!");
            return;
        }
        playerData.baseMovementSpeed += movementSpeedPoints;
        playerData.baseDamage += damagePoints;
        playerData.baseHealth += healthPoints;
        playerData.baseAttackSpeed += attackSpeedPoints;
        playerData.baseStamina += staminaPoints;
        playerData.movementSpeedLevel += movementSpeedPoints;
        playerData.damageLevel += damagePoints;
        playerData.healthLevel += healthPoints;
        playerData.attackSpeedLevel += attackSpeedPoints;
        playerData.staminaLevel += staminaPoints;
        playerData.statPointsAvailable -= totalPoints;
    }

    public void AddDamageDealt(float damage)
    {
        playerData.damageNaturalPoints += Mathf.RoundToInt(damage * 0.01f);
        ProcessDamageNaturalProgression();
    }

    public void ProcessDamageNaturalProgression()
    {
        int threshold = (playerData.damageLevel + 1) * 10;
        while (playerData.damageNaturalPoints >= threshold)
        {
            playerData.damageNaturalPoints -= threshold;
            playerData.damageLevel++;
            threshold = (playerData.damageLevel + 1) * 10;
            Debug.Log("Natural progression: Damage level increased to " + playerData.damageLevel);
        }
    }

    public void AddDistanceTravelled(float distance)
    {
        playerData.movementSpeedNaturalPoints += distance * 0.05f;
        ProcessMovementSpeedNaturalProgression();
    }

    public void ProcessMovementSpeedNaturalProgression()
    {
        int threshold = (playerData.movementSpeedLevel + 1) * 10;
        while (playerData.movementSpeedNaturalPoints >= threshold)
        {
            playerData.movementSpeedNaturalPoints -= threshold;
            playerData.movementSpeedLevel++;
            threshold = (playerData.movementSpeedLevel + 1) * 10;
            Debug.Log("Natural progression: Movement Speed level increased to " + playerData.movementSpeedLevel);
        }
    }

    public void AddHealthGain(int healthGain)
    {
        playerData.healthNaturalPoints += healthGain;
        ProcessHealthNaturalProgression();
    }

    public void ProcessHealthNaturalProgression()
    {
        int threshold = (playerData.healthLevel + 1) * 10;
        while (playerData.healthNaturalPoints >= threshold)
        {
            playerData.healthNaturalPoints -= threshold;
            playerData.healthLevel++;
            threshold = (playerData.healthLevel + 1) * 10;
            Debug.Log("Natural progression: Health level increased to " + playerData.healthLevel);
        }
    }

    public void AddAttackAction()
    {
        playerData.attackSpeedNaturalPoints += 0.1f;
        ProcessAttackSpeedNaturalProgression();
    }

    public void ProcessAttackSpeedNaturalProgression()
    {
        int threshold = (playerData.attackSpeedLevel + 1) * 10;
        while (playerData.attackSpeedNaturalPoints >= threshold)
        {
            playerData.attackSpeedNaturalPoints -= threshold;
            playerData.attackSpeedLevel++;
            threshold = (playerData.attackSpeedLevel + 1) * 10;
            Debug.Log("Natural progression: Attack Speed level increased to " + playerData.attackSpeedLevel);
        }
    }

    public void AddStaminaUsage(float usage)
    {
        playerData.staminaNaturalPoints += usage * 0.03f;
        ProcessStaminaNaturalProgression();
    }

    public void ProcessStaminaNaturalProgression()
    {
        int threshold = (playerData.staminaLevel + 1) * 10;
        while (playerData.staminaNaturalPoints >= threshold)
        {
            playerData.staminaNaturalPoints -= threshold;
            playerData.staminaLevel++;
            threshold = (playerData.staminaLevel + 1) * 10;
            Debug.Log("Natural progression: Stamina level increased to " + playerData.staminaLevel);
        }
    }

    public void ProcessAllNaturalProgressions()
    {
        ProcessDamageNaturalProgression();
        ProcessMovementSpeedNaturalProgression();
        ProcessHealthNaturalProgression();
        ProcessAttackSpeedNaturalProgression();
        ProcessStaminaNaturalProgression();
    }
}
