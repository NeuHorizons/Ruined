using UnityEngine;
using System.Collections;

public class EnemyStopAndGo : MonoBehaviour
{
    public float speed = 2f;
    public float moveTime = 2f; // Time spent moving
    public float stopTime = 1f; // Time spent stopped
    private Transform player;
    private bool isMoving = true;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        StartCoroutine(MoveCycle());
    }

    private void Update()
    {
        if (player != null && isMoving)
        {
            transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        }
    }

    IEnumerator MoveCycle()
    {
        while (true)
        {
            isMoving = true;
            yield return new WaitForSeconds(moveTime);
            isMoving = false;
            yield return new WaitForSeconds(stopTime);
        }
    }
}