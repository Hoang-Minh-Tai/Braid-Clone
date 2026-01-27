using UnityEngine;

public class PlayerState_Jump : PlayerState_Air
{

    public PlayerState_Jump(StateMachine stateMachine) : base(stateMachine, "")
    {
        type = PlayerStateEnum.Jump;
    }

    public override void Enter()
    {
        base.Enter();
        animator.SetTrigger("jump");
        if (Mathf.Abs(rb.linearVelocityX) > 0.5f)
        {
            animator.SetBool("sideJump", true);
        }

    }

    public override void Update()
    {
        base.Update();
        if (rb.linearVelocityY < 0)
        {
            stateMachine.ChangeState(player.fallState);
            return;
        }

    }

    public override void Exit()
    {
        base.Exit();
        animator.SetBool("chainJump", false);
        animator.SetBool("sideJump", false);
    }
}
