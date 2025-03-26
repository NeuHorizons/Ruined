using UnityEngine;
using System.Collections;

public class EnemyJittery : MonoBehaviour
{
    public float speed = 2f;
    public float changeDirectionTime = 1f;
    private Vector2 moveDirection;
    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(ChangeDirection());
    }

    private void Update()
    {
        if (player != null)
        {
            transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
        }
    }

    IEnumerator ChangeDirection()
    {
        while (true)
        {
            if (player != null)
            {
                // Get a random direction towards the player
                Vector2 directionToPlayer = (player.position - transform.position).normalized;
                moveDirection = (directionToPlayer + Random.insideUnitCircle).normalized;
            }
            yield return new WaitForSeconds(changeDirectionTime);
        }
    }
}