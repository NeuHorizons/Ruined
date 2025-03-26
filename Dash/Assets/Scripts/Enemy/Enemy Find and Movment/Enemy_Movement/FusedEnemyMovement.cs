using UnityEngine;
using System.Collections;

public class FusedEnemyMovement : MonoBehaviour
{
    public float slowSpeed = 1f; // Speed when not charging
    public float chargeSpeedMax = 10f; // Maximum speed during charge
    public float chargeBuildUpTime = 2f; // Time to reach max speed
    public float chargeDuration = 2f; // Time spent charging
    public float pauseBeforeCharge = 1f; // Time before charge starts
    public float postChargePause = 3f; // Time after charge before moving again
    public float chargeDetectionRange = 5f; // Distance at which it decides to charge
    public float chargeChance = 0.5f; // 50% chance to charge
    public float chargeCooldown = 3f; // Time before checking charge chance again
    public float chargeCorrectionFactor = 0.1f; // Small course correction factor
    public float minDistanceForCorrection = 3f; // If closer than this, no corrections

    private Transform player;
    private Rigidbody2D rb;
    private bool isCharging = false;
    private bool isPaused = false;
    private bool chargeCheckOnCooldown = false;
    private Vector2 chargeDirection;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(MovementLoop());
    }

    private IEnumerator MovementLoop()
    {
        while (true)
        {
            if (player == null) yield break;

            // Slow movement when not charging
            while (!isCharging && !isPaused && !chargeCheckOnCooldown)
            {
                MoveTowardsPlayer(slowSpeed);

                // If the player is within charge range and cooldown is over, roll charge chance
                if (Vector2.Distance(transform.position, player.position) <= chargeDetectionRange)
                {
                    chargeCheckOnCooldown = true; // Prevent immediate re-check
                    if (Random.value < chargeChance) // Check if charge should happen
                    {
                        StartCoroutine(ChargeSequence());
                    }
                    StartCoroutine(ChargeCooldown()); // Start cooldown regardless of outcome
                    break;
                }
                yield return null;
            }

            yield return null;
        }
    }

    private void MoveTowardsPlayer(float speed)
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }

    private IEnumerator ChargeSequence()
    {
        isCharging = true;
        rb.velocity = Vector2.zero; // Stop movement before charging
        yield return new WaitForSeconds(pauseBeforeCharge); // Pause before charging

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Lock in initial charge direction
        chargeDirection = (player.position - transform.position).normalized;

        // Only allow correction if far enough away
        bool canCorrect = distanceToPlayer > minDistanceForCorrection;

        // Gradually build up speed
        float elapsedTime = 0f;
        while (elapsedTime < chargeBuildUpTime)
        {
            float currentSpeed = Mathf.Lerp(slowSpeed, chargeSpeedMax, elapsedTime / chargeBuildUpTime);
            
            // Small adjustments to charge direction if allowed
            if (canCorrect && player != null)
            {
                Vector2 newDirection = (player.position - transform.position).normalized;
                chargeDirection = Vector2.Lerp(chargeDirection, newDirection, chargeCorrectionFactor);
            }

            rb.velocity = chargeDirection * currentSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Maintain max speed for charge duration (no more corrections)
        rb.velocity = chargeDirection * chargeSpeedMax;
        yield return new WaitForSeconds(chargeDuration);

        // Stop after charge
        rb.velocity = Vector2.zero;
        isCharging = false;
        isPaused = true;
        yield return new WaitForSeconds(postChargePause); // Pause after charge
        isPaused = false;
    }

    private IEnumerator ChargeCooldown()
    {
        yield return new WaitForSeconds(chargeCooldown);
        chargeCheckOnCooldown = false; // Now it can roll again
    }
}
