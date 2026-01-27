using UnityEngine;

public class PlayerState_Run : PlayerState_Ground
{
    public PlayerState_Run(StateMachine stateMachine) : base(stateMachine, "Ground")
    {
        type = PlayerStateEnum.Run;
    }

    public override void Enter()
    {
        base.Enter();
        movement.Flip();
    }

    public override void Update()
    {
        base.Update();
        movement.SetMoveInput(new Vector2(moveInput.x, 0));
        movement.Flip(moveInput.x);

        if (moveInput.x == 0)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

    }

    public override void Exit()
    {
        base.Exit();
    }
}
