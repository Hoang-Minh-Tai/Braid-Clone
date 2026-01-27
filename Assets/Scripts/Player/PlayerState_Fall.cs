using UnityEngine;

public class PlayerState_Fall : PlayerState_Air
{
    private float jumpPressTime; // Tracks the time the Jump button is pressed
    private bool jumpPressed; // Tracks if the Jump button was pressed
    private bool landHard;

    public PlayerState_Fall(StateMachine stateMachine, string stateName = "Fall") : base(stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        jumpPressTime = 0f;
        jumpPressed = false;
        landHard = false;
    }

    public override void Update()
    {
        base.Update();
        if (rb.linearVelocityY < -2) landHard = true;

        // Track Jump button press
        if (input.Player.Jump.triggered)
        {
            jumpPressed = true;
            jumpPressTime = Time.time; // Record the time when Jump is pressed
        }

        // Handle falling logic and transitions
        movement.Flip(moveInput.x);

        if (movement.IsGrounded)
        {
            if (landHard) audioPlayer.Play("land");
            // Check if the Jump button was pressed recently
            if (jumpPressed && Time.time - jumpPressTime <= 0.2f) // 0.2 seconds threshold
            {
                stateMachine.ChangeState(player.jumpState); // Transition to Jump state
                animator.SetBool("chainJump", true);
                movement.Jump();
                return;
            }
            else
            {
                // Transition to grounded state
                if (moveInput.x == 0)
                {
                    stateMachine.ChangeState(player.idleState);
                    rb.linearVelocityX = Mathf.Clamp(rb.linearVelocityX, -1f, 1f);
                }
                else
                    stateMachine.ChangeState(player.runState);
            }
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Apply any physics-related logic for falling here
    }

    public override void Exit()
    {
        base.Exit();
        jumpPressed = false; // Reset the jumpPressed flag
    }
}
