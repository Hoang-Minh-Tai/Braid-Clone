using UnityEngine;

public class PlayerState_ClimbNet : PlayerState_Climb
{
    public PlayerState_ClimbNet(StateMachine stateMachine, string stateName = "Climb")
        : base(stateMachine, stateName)
    {
        type = PlayerStateEnum.ClimbNet;
    }

    public override void Enter()
    {
        base.Enter();
        animator.SetFloat("x", moveInput.x);
        animator.SetFloat("y", moveInput.y);
    }

    public override void Update()
    {
        base.Update();
        if (!player.OnNet)
        {
            return;
        }

        Vector2 pos = player.transform.position;

        var net = player.currentNet;   // assign this when entering detect collider
        if (!net)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (movement.IsWallAhead && moveInput.x == movement.DirectionInt)
        {
            moveInput.x = 0;
        }

        // -----------------------------------------
        // 1. CANCEL climbing if grounded & moving down
        // -----------------------------------------
        if (moveInput.y < 0 && movement.IsGrounded)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        // -----------------------------------------
        // 2. Horizontal Climb Limit Check
        // -----------------------------------------
        if (moveInput.x < 0 && pos.x <= net.leftPoint.position.x)
        {
            animator.SetTrigger("jump");
            stateMachine.ChangeState(player.fallState);
            return;
        }

        if (moveInput.x > 0 && pos.x >= net.rightPoint.position.x)
        {
            animator.SetTrigger("jump");
            stateMachine.ChangeState(player.fallState);
            return;
        }

        // -----------------------------------------
        // 3. Vertical Climb Limit Check
        // -----------------------------------------
        if (moveInput.y > 0 && pos.y >= net.topPoint.position.y)
        {
            moveInput.y = 0;     // stop at top
        }

        if (moveInput.y < 0 && pos.y <= net.bottomPoint.position.y)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        // -----------------------------------------
        // 4. Player still climbing: apply move inputs
        // -----------------------------------------
        movement.SetMoveInput(moveInput);

        animator.SetFloat("x", moveInput.x);
        animator.SetFloat("y", moveInput.y);

        if (moveInput.x != 0) animator.SetFloat("animation_speed", 1);
        else animator.SetFloat("animation_speed", moveInput.y);

        // Flip horizontally when player moves sideways
        if (moveInput.x != 0)
            movement.Flip(moveInput.x);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
    }
}
