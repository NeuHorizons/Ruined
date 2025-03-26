using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public StatManager statManager;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    public float dashSpeed = 80f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 0.5f;
    private bool isDashing = false;
    private float nextDashTime = 0f;

    void Start()
    {
        if (statManager == null)
        {
            Debug.LogError("StatManager is not assigned in PlayerMovement!");
            return;
        }
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        MovePlayer();
        RotateTowardsMouse();
        if (statManager.playerData.dashUnlocked && Input.GetKeyDown(KeyCode.Space) && Time.time >= nextDashTime)
            StartCoroutine(Dash());
    }

    void MovePlayer()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(moveX, moveY).normalized;
    }

    void FixedUpdate()
    {
        if (!isDashing)
            rb.velocity = moveInput * statManager.FinalMovementSpeed;
    }

    void RotateTowardsMouse()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    IEnumerator Dash()
    {
        isDashing = true;
        nextDashTime = Time.time + dashCooldown;
        Vector2 dashDirection = moveInput;
        if (dashDirection == Vector2.zero)
            dashDirection = transform.right;
        rb.velocity = dashDirection * dashSpeed;
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }
}