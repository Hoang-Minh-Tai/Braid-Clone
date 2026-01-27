using UnityEngine;

public class Player_Movement : MonoBehaviour
{
    public enum Direction
    {
        Left,
        Right,
    }

    private Vector2 moveInput;
    private Rigidbody2D rb;
    private Direction direction;

    [Header("Editor Settings")]
    public bool flipX = false;

    [Header("Material Settings")]
    [SerializeField] private PhysicsMaterial2D groundMaterial;
    [SerializeField] private PhysicsMaterial2D airMaterial;

    [Header("Movement Detail")]
    [SerializeField] private float speed = 2;
    [SerializeField] private float maxSpeed = 5;
    [SerializeField] private float jumpForce = 5;
    [SerializeField] public float mobJumpAddForce = 1;
    [SerializeField] private float airControlForce = 0.2f;
    [SerializeField] private float climbSpeed = 2;
    [SerializeField] private float acceleration = 5f;
    [SerializeField] private float deceleration = 5f;
    [SerializeField] private float jumpForceLimit = 5;

    [Header("Collision Check")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private float feetSpacing = 0.2f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private LayerMask platformMask;
    [SerializeField] private Transform groundCheckPoint;

    [Header("Wall Check (Hedgehog-style)")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private float wallCheckDistance = 0.3f;
    [SerializeField] private LayerMask wallMask;

    public Vector2 Velocity => rb.linearVelocity;
    public Vector2 MoveInput => moveInput;
    public int DirectionInt => direction == Direction.Right ? 1 : -1;

    public bool IsGrounded { get; private set; }
    public bool IsOnPlatform { get; private set; }
    public bool IsWallAhead { get; private set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        CheckGroundCollision();
        CheckPlatformCollision();
        CheckWallCollision();

        // Physics material switching
        if (rb.gravityScale == 0 || Mathf.Abs(rb.linearVelocityY) > 0.1f)
            rb.sharedMaterial = airMaterial;
        else
            rb.sharedMaterial = groundMaterial;

        // Movement
        if (rb.gravityScale == 0)
        {
            Vector2 normalizedInput = moveInput.normalized;
            rb.linearVelocity = new Vector2(normalizedInput.x * speed * 0.75f, normalizedInput.y * climbSpeed);
        }
        else if (IsGrounded)
        {
            ApplyAcceleration();
        }
        else
        {
            if (moveInput.x != 0)
                rb.AddForceX(moveInput.x * airControlForce);
        }

        // Clamp horizontal speed
        if (Mathf.Abs(rb.linearVelocityX) > maxSpeed)
        {
            rb.linearVelocityX = Mathf.Sign(rb.linearVelocityX) * maxSpeed;
            if (!IsGrounded) rb.linearVelocityX *= 0.7f;
        }

        rb.linearVelocityY = Mathf.Clamp(rb.linearVelocityY, -10, jumpForceLimit);

    }

    private void ApplyAcceleration()
    {
        float targetSpeed = moveInput.x * speed;
        float currentSpeed = rb.linearVelocity.x;

        int facingDir = direction == Direction.Right ? 1 : -1;

        if (IsWallAhead)
        {
            rb.linearVelocityX = 0;
            return;
        }
        if (Mathf.Abs(targetSpeed) > 0.01f)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                acceleration * Time.fixedDeltaTime
            );
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                0,
                deceleration * Time.fixedDeltaTime
            );
        }

        rb.linearVelocity = new Vector2(currentSpeed, rb.linearVelocity.y);
    }

    public void Jump()
    {
        rb.sharedMaterial = airMaterial;
        rb.linearVelocityY = jumpForce;
    }

    public void MobJump()
    {
        float yForce = Mathf.Abs(rb.linearVelocityY) + mobJumpAddForce;
        yForce = Mathf.Clamp(yForce, jumpForce, jumpForceLimit);

        rb.sharedMaterial = airMaterial;
        rb.linearVelocityY = yForce;

        Debug.Log($"Y force applied: {yForce}");
        Debug.Log($"MobJump with force: {rb.linearVelocity}");
    }

    public void SetMoveInput(Vector2 moveInput)
    {
        this.moveInput = moveInput;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;

        Vector3 scale = transform.localScale;
        scale.x = flipX ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        direction = flipX ? Direction.Left : Direction.Right;
    }
#endif
    public void FlipForce(float scaleX)
    {
        Vector3 scale = transform.localScale;
        scale.x = scaleX;
        transform.localScale = scale;

        direction = scaleX > 0 ? Direction.Right : Direction.Left;
    }

    public void Flip(Direction newDirection)
    {
        if (newDirection == direction) return;

        direction = newDirection;
        Vector3 scale = transform.localScale;
        scale.x = (newDirection == Direction.Right)
            ? Mathf.Abs(scale.x)
            : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    public void Flip(float velocityX)
    {
        if (Mathf.Abs(velocityX) < 0.3f) return;
        Flip(velocityX > 0 ? Direction.Right : Direction.Left);
    }

    public void Flip()
    {
        if (Mathf.Abs(rb.linearVelocityX) < 1f) return;
        Flip(rb.linearVelocityX);
    }

    private void CheckGroundCollision()
    {
        LayerMask mask = rb.gravityScale == 0 ? groundMask : groundMask | platformMask;

        IsGrounded =
            Physics2D.Raycast(groundCheckPoint.position + Vector3.left * feetSpacing, Vector2.down, groundCheckDistance, mask) ||
            Physics2D.Raycast(groundCheckPoint.position + Vector3.right * feetSpacing, Vector2.down, groundCheckDistance, mask);
    }

    private void CheckPlatformCollision()
    {
        IsOnPlatform = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            platformMask
        );
    }

    // ✅ Hedgehog-style wall check
    private void CheckWallCollision()
    {
        Vector2 dir = direction == Direction.Right ? Vector2.right : Vector2.left;

        IsWallAhead = Physics2D.Raycast(
            wallCheckPoint.position,
            dir,
            wallCheckDistance,
            wallMask
        );
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = IsGrounded ? Color.green : Color.red;

            Gizmos.DrawLine(
                groundCheckPoint.position + Vector3.left * feetSpacing,
                groundCheckPoint.position + Vector3.left * feetSpacing + Vector3.down * groundCheckDistance
            );

            Gizmos.DrawLine(
                groundCheckPoint.position + Vector3.right * feetSpacing,
                groundCheckPoint.position + Vector3.right * feetSpacing + Vector3.down * groundCheckDistance
            );
        }

        if (wallCheckPoint != null)
        {
            Gizmos.color = IsWallAhead ? Color.red : Color.gray;
            Vector3 dir = direction == Direction.Right ? Vector3.right : Vector3.left;

            Gizmos.DrawLine(
                wallCheckPoint.position,
                wallCheckPoint.position + dir * wallCheckDistance
            );
        }
    }
}
