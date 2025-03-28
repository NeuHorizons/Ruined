using UnityEngine;

public class EnemyChase : MonoBehaviour
{
    private Transform player;
    private Enemy enemy; // Reference to the enemy script

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        enemy = GetComponent<Enemy>();

        if (enemy == null)
        {
            Debug.LogError("EnemyChase requires an Enemy component on the same GameObject.");
        }
    }

    private void Update()
    {
        if (player != null && enemy != null)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, enemy.finalSpeed * Time.deltaTime);
        }
    }
}