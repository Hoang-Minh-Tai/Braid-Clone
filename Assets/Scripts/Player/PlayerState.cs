using UnityEngine;

public abstract class PlayerState
{
    protected StateMachine stateMachine;
    protected string stateName;
    protected Player_Movement movement;
    protected Animator animator;
    protected Player player;
    protected Rigidbody2D rb;
    protected PlayerControl input;
    protected SpriteRenderer rd;

    public PlayerStateEnum type;
    protected Vector2 moveInput;
    protected RewindableAudioPlayer audioPlayer;

    public PlayerState(StateMachine stateMachine, string stateName = "")
    {
        this.stateMachine = stateMachine;
        this.stateName = stateName;

        player = Player.Instance;
        animator = player.animator;
        movement = player.movement;
        rb = player.rb;
        rd = player.rd;
        input = player.input;
        audioPlayer = player.audioPlayer;

    }


    public virtual void Enter()
    {
        if (!string.IsNullOrEmpty(stateName)) animator.SetBool(stateName, true);
    }

    public virtual void Update()
    {
        moveInput = input.Player.Move.ReadValue<Vector2>();
        if (!stateMachine.enableMovement) moveInput = Vector2.zero;
    }

    public virtual void FixedUpdate()
    {

    }

    public virtual void Exit()
    {
        if (!string.IsNullOrEmpty(stateName)) animator.SetBool(stateName, false);
    }

    public virtual void SetRewindSpeed(int speed)
    {

    }
}
