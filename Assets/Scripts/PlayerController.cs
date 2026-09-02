using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float groundAcceleration = 60f;
    [SerializeField] private float groundDeceleration = 70f;
    [SerializeField] private float airAcceleration = 40f;
    [SerializeField] private float airDeceleration = 30f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckDistance = 0.08f;

    [Header("Hazard")]
    [SerializeField] private LayerMask hazardLayer;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;

    private float horizontalInput;
    private bool jumpRequested;
    private bool isGrounded;
    private bool controlEnabled = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (hazardLayer.value == 0)
        {
            hazardLayer = LayerMask.GetMask("Hazard");
        }

        if (groundLayer.value == 0)
        {
            groundLayer = ~hazardLayer;
        }
    }

    void Update()
    {
        if (!controlEnabled)
        {
            horizontalInput = 0f;
            return;
        }

        ReadInput();
    }

    void FixedUpdate()
    {
        CheckGrounded();

        if (isGrounded && controlEnabled)
        {
            GameManager instanceCheck = GameManager.Instance;
            if (instanceCheck != null)
            {
                instanceCheck.UpdateSafePosition(transform.position);
            }
        }

        ApplyHorizontalMovement();
        ApplyJump();
    }

    private void ReadInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump"))
        {
            jumpRequested = true;
        }
    }

    private void CheckGrounded()
    {
        Bounds bounds = boxCollider.bounds;
        Vector2 origin = new Vector2(bounds.center.x, bounds.min.y);
        Vector2 size = new Vector2(bounds.size.x * 0.9f, 0.05f);

        RaycastHit2D hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, groundCheckDistance, groundLayer);
        isGrounded = hit.collider != null;
    }

    private void ApplyHorizontalMovement()
    {
        float targetSpeed = controlEnabled ? horizontalInput * moveSpeed : 0f;
        float speedDifference = targetSpeed - rb.linearVelocity.x;

        float accelerationRate = Mathf.Abs(targetSpeed) > 0.01f
            ? (isGrounded ? groundAcceleration : airAcceleration)
            : (isGrounded ? groundDeceleration : airDeceleration);

        float movement = speedDifference * accelerationRate * Time.fixedDeltaTime;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    // Jump strength is currently fixed; ExecuteJump reads GetJumpVelocity() so a future
    // hold-to-jump feature can override jump height without touching the call site.
    private void ApplyJump()
    {
        if (!jumpRequested)
        {
            return;
        }

        jumpRequested = false;

        if (!isGrounded || !controlEnabled)
        {
            return;
        }

        ExecuteJump();
    }

    private void ExecuteJump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, GetJumpVelocity());
    }

    protected virtual float GetJumpVelocity()
    {
        return jumpForce;
    }

    public void SetControlEnabled(bool value)
    {
        controlEnabled = value;

        if (!value)
        {
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void Respawn(Vector3 position)
    {
        transform.position = position;
        rb.linearVelocity = Vector2.zero;
        jumpRequested = false;
        SetControlEnabled(true);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & hazardLayer.value) == 0)
        {
            return;
        }

        SetControlEnabled(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDeath();
        }
    }
}
