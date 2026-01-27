using UnityEngine;

public class PlayerState_ClimbLadder : PlayerState_Climb
{

    public PlayerState_ClimbLadder(StateMachine stateMachine, string stateName = "Climb") : base(stateMachine, stateName)
    {
        type = PlayerStateEnum.ClimbLadder;
    }

    public override void Enter()
    {
        base.Enter();
        animator.SetFloat("x", moveInput.x);
    }

    public override void Update()
    {
        base.Update();

        if (player.currentLadder == null) return;

        if (moveInput.x != 0)
        {
            movement.Flip(moveInput.x);

            stateMachine.ChangeState(player.idleState);
            return;
        }

        // Get off ladder when touch ground
        if (moveInput.y < 0 && movement.IsGrounded)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        // Limit climb height
        if (player.transform.position.y >= player.currentLadder.topPoint.transform.position.y && moveInput.y > 0)
        {
            moveInput.y = 0;
        }

        else if (player.transform.position.y <= player.currentLadder.bottomPoint.transform.position.y && moveInput.y < 0)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

        animator.SetFloat("y", moveInput.y);
        movement.SetMoveInput(moveInput);
        animator.SetFloat("animation_speed", moveInput.y);

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
