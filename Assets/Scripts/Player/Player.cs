using UnityEngine;
using UnityEngine.Events;

public enum PlayerStateEnum
{
    Idle,
    Run,
    Jump,
    Look,
    TimeRewind,
    Dead,
    ClimbLadder,
    Fall,
    ClimbNet
}

public class Player : MonoBehaviour
{
    public static Player Instance { get; private set; }
    [HideInInspector]
    public Animator animator;
    [HideInInspector] public Player_Movement movement;
    [HideInInspector] public PlayerControl input;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public SpriteRenderer rd;
    public RewindableAudioPlayer audioPlayer;

    public DynamicLadder currentLadder;
    public ClimbingNet currentNet;
    public MovingPlatform currentPlatform;
    public bool landOnMob = false;

    public bool OnNet => currentNet != null;
    public bool OnLadder => currentLadder != null;
    public bool OnMovingPlatform => currentPlatform != null;
    public bool HasKey => keyAttachPoint.childCount > 0;

    public StateMachine stateMachine;
    public PlayerState_Idle idleState;
    public PlayerState_Run runState;
    public PlayerState_Jump jumpState;
    public PlayerState_Look lookState;
    public PlayerState_TimeRewind rewindState;
    public PlayerState_Dead deadState; // Added dead state
    public PlayerState_ClimbLadder climbLadderState; // Added climb ladder state
    public PlayerState_ClimbNet climbNetState; // Added climb net state
    public PlayerState_Fall fallState; // Added fall state

    public UnityEvent<bool> showDoorUIEvent;

    public Door currentDoor;
    public Transform keyAttachPoint;
    private Player_AnimationTrigger animationTrigger;

    private void Awake()
    {
        // Singleton pattern implementation
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        animator = GetComponentInChildren<Animator>();
        rd = GetComponentInChildren<SpriteRenderer>();
        movement = GetComponent<Player_Movement>();
        rb = GetComponent<Rigidbody2D>();
        animationTrigger = GetComponentInChildren<Player_AnimationTrigger>();
        input = InputManager.Instance.Input;
        audioPlayer = GetComponentInChildren<RewindableAudioPlayer>();

        stateMachine = new StateMachine();
        idleState = new PlayerState_Idle(stateMachine);
        runState = new PlayerState_Run(stateMachine);
        jumpState = new PlayerState_Jump(stateMachine);
        lookState = new PlayerState_Look(stateMachine);
        rewindState = new PlayerState_TimeRewind(stateMachine);
        deadState = new PlayerState_Dead(stateMachine); // Initialize dead state
        climbLadderState = new PlayerState_ClimbLadder(stateMachine); // Initialize climb ladder state
        climbNetState = new PlayerState_ClimbNet(stateMachine); // Initialize climb net state
        fallState = new PlayerState_Fall(stateMachine); // Initialize fall state
    }

    private void Start()
    {
        stateMachine.Initialize(idleState);
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.Rewind.performed += ctx => GameEventManager.instance.generalEvent.StartRewind();

        GameEventManager.instance.generalEvent.onRewindStart.AddListener(ctx => EnterRewindState(ctx));
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void Update()
    {
        animator.SetFloat("x", movement.Velocity.x);
        animator.SetFloat("y", movement.Velocity.y);
        animator.SetBool("grounded", movement.IsGrounded);

        stateMachine.UpdateCurrent();
    }

    private void FixedUpdate()
    {
        stateMachine.FixedUpdateCurrent();
    }

    private void EnterRewindState(int rewindSpeed)
    {
        stateMachine.ChangeState(rewindState);
        rewindState.SetRewindSpeed(rewindSpeed);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("MovingPlatform"))
        {
            transform.SetParent(collision.transform);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Mob_HedgeHog enemy = collision.gameObject.GetComponent<Mob_HedgeHog>();
            // Check the angle of collision
            if (collision.contacts.Length > 0)
            {
                Vector2 collisionNormal = collision.contacts[0].normal;
                if (transform.position.y > collision.gameObject.transform.position.y && !movement.IsGrounded)
                {
                    movement.MobJump();
                    audioPlayer.Play("bounce");
                    rb.linearVelocityY = Mathf.Abs(collision.relativeVelocity.y + movement.mobJumpAddForce);


                    enemy.TriggerDead(gameObject);
                    landOnMob = true;
                    return;
                }
            }
            audioPlayer.Play("hit_spike");
            enemy.KillPlayerSound();
            stateMachine.ChangeState(deadState);
        }

        // if (collision.gameObject.CompareTag("Gate"))
        // {
        //     Gate gate = collision.gameObject.GetComponent<Gate>();
        //     gate.Open();

        //     Key key = keyAttachPoint.GetComponentInChildren<Key>();
        //     key.UseKey();
        // }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        try
        {
            if (collision.gameObject.CompareTag("MovingPlatform"))
            {
                transform.SetParent(null);
            }
        }
        catch (System.Exception)
        {
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            currentLadder = other.transform.GetComponentInParent<DynamicLadder>();
        }

        else if (other.CompareTag("Net"))
        {
            currentNet = other.transform.GetComponentInParent<ClimbingNet>();
        }

        else if (other.CompareTag("Door") && Mathf.Abs(rb.linearVelocityY) < 0.1f)
        {
            showDoorUIEvent.Invoke(true);
        }
        else if (other.CompareTag("DeadZone"))
        {
            audioPlayer.Play("hit_spike");
            stateMachine.ChangeState(deadState);
        }
        else if (other.CompareTag("MovingPlatform"))
        {
            currentPlatform = other.transform.GetComponentInParent<MovingPlatform>();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Door") && Mathf.Abs(rb.linearVelocityY) < 0.1f)
        {
            if (currentDoor == null)
            {
                Door door = other.GetComponent<Door>();
                currentDoor = door;
            }
            showDoorUIEvent.Invoke(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ladder"))
        {
            currentLadder = null;
        }
        else if (other.CompareTag("Net"))
        {
            currentNet = null;
        }
        else if (other.CompareTag("Door"))
        {
            showDoorUIEvent.Invoke(false);
            currentDoor = null;
        }
        else if (other.CompareTag("MovingPlatform"))
        {
            currentPlatform = null;
        }
    }

    public PlayerState GetStateFromEnum(PlayerStateEnum stateEnum)
    {
        return stateEnum switch
        {
            PlayerStateEnum.Idle => idleState,
            PlayerStateEnum.Run => runState,
            PlayerStateEnum.Jump => jumpState,
            PlayerStateEnum.Look => lookState,
            PlayerStateEnum.TimeRewind => rewindState,
            PlayerStateEnum.Dead => deadState,
            PlayerStateEnum.ClimbLadder => climbLadderState,
            PlayerStateEnum.ClimbNet => climbNetState,
            PlayerStateEnum.Fall => fallState,
            _ => idleState // Default fallback
        };
    }

    public void OpenCurrentDoor()
    {
        if (currentDoor != null)
        {
            currentDoor.EnterDoor();
            Destroy(gameObject);
        }
    }

    [ContextMenu("Print current state")]
    public void PrintCurrentState()
    {
        Debug.Log($"Current Player State: {stateMachine.currentStateType}");
    }


}