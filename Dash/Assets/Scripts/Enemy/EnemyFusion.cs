using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyFusion : MonoBehaviour
{
    public float fusionRadius = 2f; // Range to check for fusion
    public float fusionChance = 0.1f; // 10% chance to fuse when enough enemies are close
    public int requiredEnemiesToFuse = 5; // Number of enemies needed for fusion
    public GameObject fusedEnemyPrefab; // Assign a stronger enemy prefab in Inspector
    public float fusionCheckCooldown = 5f; // Time before checking fusion again

    private bool fusionOnCooldown = false;

    private void Start()
    {
        StartCoroutine(FusionCheckLoop());
    }

    IEnumerator FusionCheckLoop()
    {
        while (true)
        {
            if (!fusionOnCooldown)
            {
                StartCoroutine(CheckForFusion());
            }
            yield return new WaitForSeconds(fusionCheckCooldown); // Wait before next check
        }
    }

    IEnumerator CheckForFusion()
    {
        fusionOnCooldown = true; // Prevent immediate rechecking
        yield return new WaitForSeconds(0.5f); // Small delay to allow enemies to settle

        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, fusionRadius);
        List<GameObject> enemiesToMerge = new List<GameObject>();

        foreach (Collider2D collider in nearbyEnemies)
        {
            if (collider.CompareTag("Enemy") && collider.gameObject != gameObject)
            {
                enemiesToMerge.Add(collider.gameObject);
            }
        }

        if (enemiesToMerge.Count >= requiredEnemiesToFuse - 1 && Random.value < fusionChance) // -1 because it includes itself
        {
            MergeEnemies(enemiesToMerge);
        }

        yield return new WaitForSeconds(fusionCheckCooldown); // Set cooldown before checking again
        fusionOnCooldown = false;
    }

    void MergeEnemies(List<GameObject> enemiesToMerge)
    {
        // Find the center position of all merging enemies
        Vector2 fusionCenter = Vector2.zero;
        foreach (GameObject enemy in enemiesToMerge)
        {
            fusionCenter += (Vector2)enemy.transform.position;
        }
        fusionCenter /= enemiesToMerge.Count; // Get the average position

        // Destroy merging enemies
        foreach (GameObject enemy in enemiesToMerge)
        {
            Destroy(enemy);
        }
        Destroy(gameObject); // Destroy itself

        // Spawn the fused enemy at the center position
        Instantiate(fusedEnemyPrefab, fusionCenter, Quaternion.identity);
        Debug.Log("Five enemies have fused into a stronger form!");
    }
}
