using UnityEngine;
using System.Collections;

public class EnemyCircling : MonoBehaviour
{
    public float speed = 2f;
    public float circleRadius = 3f;
    public float rotationSpeed = 2f;
    public float switchSmoothSpeed = 2f; // How quickly the orbit direction transitions
    public float lungeSpeed = 8f;
    public float lungeDuration = 0.5f;
    public float minLungeDelay = 2f;
    public float maxLungeDelay = 5f;

    private Transform player;
    private Vector2 circleCenter;
    private bool isLunging = false;

    // Variables for orbiting
    private float orbitAngle = 0f; // Current angle around the player
    private float currentOrbitDirection = 1f; // 1 for clockwise, -1 for counterclockwise
    private float orbitDirectionTarget = 1f; // Target direction to smoothly transition to

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player != null)
        {
            circleCenter = player.position;
        }
        StartCoroutine(LungeRoutine());
        StartCoroutine(ChangeOrbitDirectionRoutine());
    }

    private void Update()
    {
        if (player != null && !isLunging)
        {
            // Update center to follow the player
            circleCenter = player.position;

            // Smoothly transition the orbit direction toward the target
            currentOrbitDirection = Mathf.Lerp(currentOrbitDirection, orbitDirectionTarget, Time.deltaTime * switchSmoothSpeed);
            // Increment the orbit angle using the current orbit direction
            orbitAngle += Time.deltaTime * rotationSpeed * currentOrbitDirection;
            // Calculate the offset based on the current orbit angle
            Vector2 offset = new Vector2(Mathf.Cos(orbitAngle), Mathf.Sin(orbitAngle)) * circleRadius;
            // Smoothly move toward the target position around the player
            transform.position = Vector2.Lerp(transform.position, (Vector2)circleCenter + offset, speed * Time.deltaTime);
        }
    }

    IEnumerator LungeRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minLungeDelay, maxLungeDelay));
            if (player != null)
            {
                StartCoroutine(LungeAtPlayer());
            }
        }
    }

    IEnumerator LungeAtPlayer()
    {
        isLunging = true;
        Vector2 lungeDirection = (player.position - transform.position).normalized;
        float lungeTime = 0f;
        while (lungeTime < lungeDuration)
        {
            transform.position += (Vector3)(lungeDirection * lungeSpeed * Time.deltaTime);
            lungeTime += Time.deltaTime;
            yield return null;
        }
        isLunging = false;
    }

    IEnumerator ChangeOrbitDirectionRoutine()
    {
        while (true)
        {
            // Wait for a random time before possibly flipping the orbit direction
            yield return new WaitForSeconds(Random.Range(3f, 6f));
            // With a 50% chance, flip the target orbit direction
            if (Random.value < 0.5f)
            {
                orbitDirectionTarget = -orbitDirectionTarget;
            }
        }
    }
}
