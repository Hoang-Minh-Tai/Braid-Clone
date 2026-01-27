using System;
using UnityEngine;
using UnityEngine.Events;

public class Mob_HedgeHog : MonoBehaviour
{
    [HideInInspector]
    public Rigidbody2D rb;
    private Collider2D cld;
    private Animator animator;
    private HedgeHog_TimeTrack timeTrack;
    private RewindableAudioPlayer audioPlayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    private int moveDirection = 1; // 1 = right, -1 = left

    [Header("Collision Check")]
    [SerializeField] private Transform wallCheckPoint;
    [SerializeField] private float wallCheckDistance = 0.5f;
    [SerializeField] private Transform cliffCheckPoint;
    [SerializeField] private float cliffCheckDistance = 0.5f;
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool skipCliffCheck = false;

    private bool onMovingPlatform;

    [Header("Status")]
    public bool isWallAhead;
    public bool isCliffAhead;
    public bool isGrounded;
    public Transform keyAttachPoint;

    [Header("Editor Only")]
    public bool flipX = false;

    [Header("Death Settings")]
    [SerializeField] private bool startDead = false; // Option to start in a dead state
    [SerializeField] private float fallOutOfScreenY = -10f; // Y position threshold for falling out of the screen

    [Space]
    public UnityEvent onDeadEvent;

    private bool _dead;
    public bool Dead
    {
        get => _dead;
        set
        {
            _dead = value;
            rb.freezeRotation = !_dead;
            animator.SetBool("Dead", _dead);
            cld.enabled = !_dead; // Disable collider when dead is true
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        cld = GetComponent<Collider2D>();
        animator = GetComponentInChildren<Animator>();
        timeTrack = GetComponent<HedgeHog_TimeTrack>();
        audioPlayer = GetComponentInChildren<RewindableAudioPlayer>();

        // Set the initial state based on the `startDead` option
        Dead = startDead;
    }

    private void Start()
    {
        if (flipX)
            SetDirection(-1);
        else
            SetDirection(1);
    }

    private void FixedUpdate()
    {
        CheckFallOutOfScreen();
        if (RewindTimeManager.instance.rewinding || Dead) return;

        CheckCollisions();
        HandleMovement();
    }

    private void CheckFallOutOfScreen()
    {
        if (RewindTimeManager.instance.rewinding) return;
        if (transform.position.y < fallOutOfScreenY)
        {
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
        }
        else
        {
            rb.gravityScale = 1;
        }
    }

    public void TriggerDead(GameObject source)
    {
        Dead = true;
        onDeadEvent.Invoke();
        animator.SetBool("Dead", true);
        rb.freezeRotation = false;

        if (source.CompareTag("Player"))
        {
            audioPlayer.Play("die");
        }
        else
        {
            rb.linearVelocityY = 0.5f;
            rb.linearVelocityX = 0f;
            if (Mathf.Abs(Vector2.Distance(Player.Instance.transform.position, transform.position)) < 10f)
                audioPlayer.Play("hit_spike");
        }
    }

    private void CheckCollisions()
    {
        if (Mathf.Abs(rb.linearVelocityY) > 0.1f) return;
        Vector2 dir = Vector2.right * moveDirection;

        // Wall check
        isWallAhead = Physics2D.Raycast(
            wallCheckPoint.position,
            dir,
            wallCheckDistance,
            groundLayer
        );

        // Cliff check (no ground under cliffCheckPoint)
        isCliffAhead = skipCliffCheck ? false : !Physics2D.Raycast(
            cliffCheckPoint.position,
            Vector2.down,
            cliffCheckDistance,
            groundLayer
        );

        // Grounded check
        isGrounded = Physics2D.Raycast(
            groundCheckPoint.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(collision.transform);
            onMovingPlatform = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(null);
            onMovingPlatform = false;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("DeadZone"))
        {
            TriggerDead(collision.gameObject);
        }
    }

    private void HandleMovement()
    {
        animator.SetFloat("velocityY", rb.linearVelocity.y);
        // Only flip if on ground (don’t flip mid-air)
        if (isGrounded)
        {
            if (isCliffAhead && !onMovingPlatform) Flip();
            if (isWallAhead) Flip();
        }


        // Apply horizontal movement
        if (isGrounded)
        {
            rb.linearVelocity = new Vector2(moveDirection * moveSpeed, rb.linearVelocity.y);
        }
        else
        {
            if (moveDirection != Mathf.Sign(rb.linearVelocityX))
                Flip();
            if (Mathf.Abs(rb.linearVelocityX) < moveDirection * moveSpeed)
                rb.linearVelocityX = moveDirection * moveSpeed;
        }
    }

    private void Flip()
    {
        moveDirection *= -1;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;
    }

    public void SetDirection(float direction)
    {
        moveDirection = direction > 0 ? 1 : -1;
        Vector3 localScale = transform.localScale;
        localScale.x = Mathf.Abs(localScale.x) * moveDirection;
        transform.localScale = localScale;
    }

    public void KillPlayerSound()
    {
        audioPlayer.Play("kill_player");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (wallCheckPoint != null)
        {
            Gizmos.color = isWallAhead ? Color.red : Color.gray;
            Vector3 dir = Vector3.right * (moveDirection != 0 ? moveDirection : 1);
            Gizmos.DrawLine(wallCheckPoint.position, wallCheckPoint.position + dir * wallCheckDistance);
        }

        if (cliffCheckPoint != null)
        {
            Gizmos.color = isCliffAhead ? Color.yellow : Color.cyan;
            Gizmos.DrawLine(cliffCheckPoint.position, cliffCheckPoint.position + Vector3.down * cliffCheckDistance);
        }

        if (groundCheckPoint != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.magenta;
            Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
        }
    }

#endif

    private void OnValidate()
    {
        // Auto-set direction based on flipX
        if (flipX)
            SetDirection(-1);
        else
            SetDirection(1);
    }
}
