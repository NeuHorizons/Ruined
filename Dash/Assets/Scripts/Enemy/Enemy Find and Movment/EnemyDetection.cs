using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyDetection : MonoBehaviour
{
    [Header("Detection Settings")]
    public float activationDistance = 8f;
    public LayerMask playerLayer;

    [Header("Movement & AI Scripts")]
    public MonoBehaviour movementScript; 
    public MonoBehaviour aiPathScript;   

    [Header("Alert Settings")]
    
    public bool canTriggerGlobalAlert = false;
    
    public float alertRadius = 10f;
    
   
    private bool alertCalled = false;
   
    public bool isAlerted = false;

    private Transform player;
    public static bool hiveMindAlert = false; 
    public static List<EnemyDetection> allEnemies = new List<EnemyDetection>();

    private void Awake()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
        else
            Debug.LogWarning("Player not found. Make sure the player has the tag 'Player'.");
    }

    private void OnEnable()
    {
        if (!allEnemies.Contains(this))
            allEnemies.Add(this);

        
        if (movementScript != null)
            movementScript.enabled = false;
        if (aiPathScript != null)
            aiPathScript.enabled = false;

        
        if (canTriggerGlobalAlert && hiveMindAlert)
        {
            ActivateChase();
        }
        else
        {
            
            StartCoroutine(CheckDetectionOnEnable());
        }
    }

    private IEnumerator CheckDetectionOnEnable()
    {
        yield return new WaitForEndOfFrame();

        if (IsPlayerInRange() && !alertCalled)
        {
            Debug.Log(gameObject.name + " detects player on enable.");
            if (canTriggerGlobalAlert)
            {
                AlertAllEnemies();
            }
            else
            {
                AlertNearbyEnemies();
            }
            alertCalled = true;
            ActivateChase();
        }
    }

    private void Update()
    {
        
        if (canTriggerGlobalAlert && hiveMindAlert)
        {
            ActivateChase();
            return;
        }

        if (IsPlayerInRange() && !alertCalled)
        {
            Debug.Log(gameObject.name + " sees the player within activation distance.");
            if (canTriggerGlobalAlert)
            {
                AlertAllEnemies();
            }
            else
            {
                AlertNearbyEnemies();
            }
            alertCalled = true;
            ActivateChase();
        }
    }

    
    private bool IsPlayerInRange()
    {
        if (player == null)
            return false;

        float distance = Vector3.Distance(player.position, transform.position);
        return distance <= activationDistance;
    }

    
    public static void AlertAllEnemies()
    {
        hiveMindAlert = true;
        foreach (EnemyDetection enemy in allEnemies)
        {
            enemy.ActivateChase();
        }
    }

    
    public void AlertNearbyEnemies()
    {
        foreach (EnemyDetection enemy in allEnemies)
        {
            if (enemy != null && enemy != this && Vector3.Distance(transform.position, enemy.transform.position) <= alertRadius)
            {
                enemy.ActivateChase();
            }
        }
    }

    public static void ResetHiveMind()
    {
        hiveMindAlert = false;
        allEnemies.Clear();
    }

   
    public void ActivateChase()
    {
        isAlerted = true;
        if (movementScript != null)
            movementScript.enabled = true;
        
    }

    public void DeactivateChase()
    {
        isAlerted = false;
        if (movementScript != null)
            movementScript.enabled = false;
        if (aiPathScript != null)
            aiPathScript.enabled = false;
    }

    private void OnDisable()
    {
        if (allEnemies.Contains(this))
            allEnemies.Remove(this);
    }

    private void OnDestroy()
    {
        if (allEnemies.Contains(this))
            allEnemies.Remove(this);
    }
}
