using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float baseFollowSpeed = 10f; 
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;

    private Rigidbody2D playerRb;
    private float dynamicSpeed;
    private bool playerFound = false; // ✅ Tracks if the player has been found

    private void Start()
    {
        InvokeRepeating(nameof(FindPlayer), 0f, 0.5f); // ✅ Keeps checking every 0.5s until found
    }

    private void FixedUpdate()
    {
        if (player == null) return;

        // Adjust camera speed dynamically based on player movement
        float playerSpeed = playerRb != null ? playerRb.velocity.magnitude : 0f;
        dynamicSpeed = Mathf.Max(baseFollowSpeed, playerSpeed * 1.5f); 

        Vector3 targetPosition = player.position + offset;

        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, dynamicSpeed * Time.fixedDeltaTime);
    }

    private void FindPlayer()
    {
        if (playerFound) return; // ✅ Stops checking once the player is found

        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");

        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
            playerRb = player.GetComponent<Rigidbody2D>();
            playerFound = true; // ✅ Stops searching for the player

            if (playerRb == null)
            {
                Debug.LogError("Player found but missing Rigidbody2D. Camera may not follow properly.");
            }
            else
            {
                Debug.Log("✅ Player found! Camera following.");
            }

            // ✅ Instantly snap camera to player position on first detection
            transform.position = player.position + offset;
            CancelInvoke(nameof(FindPlayer)); // ✅ Stops repeating search
        }
    }
}
