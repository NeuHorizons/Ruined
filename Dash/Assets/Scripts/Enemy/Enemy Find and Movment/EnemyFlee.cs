using UnityEngine;
using System.Collections;

public class EnemyFlee : MonoBehaviour
{
    public float fleeSpeed = 5f;
    public float fleeDuration = 2f;

    private Rigidbody2D rb;
    private bool isFleeing = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void StartFleeing(Vector2 fleeDirection)
    {
        if (!isFleeing)
        {
            StartCoroutine(FleeRoutine(fleeDirection));
        }
    }

    private IEnumerator FleeRoutine(Vector2 fleeDirection)
    {
        isFleeing = true;
        float timer = fleeDuration;

        while (timer > 0)
        {
            rb.velocity = fleeDirection * fleeSpeed;
            timer -= Time.deltaTime;
            yield return null;
        }

        rb.velocity = Vector2.zero;
        isFleeing = false;

        EnemyDetection.AlertAllEnemies(); // Once done fleeing, alert others
    }
}