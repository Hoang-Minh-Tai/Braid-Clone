using UnityEngine;

public class PlayerState_Idle : PlayerState_Ground
{
    public PlayerState_Idle(StateMachine stateMachine) : base(stateMachine, "Ground") {
        type = PlayerStateEnum.Idle;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();


        if (moveInput.x != 0)
        {
            stateMachine.ChangeState(player.runState);
            return;
        }

        if (input.Player.Look.ReadValue<float>() != 0)
        {
            stateMachine.ChangeState(player.lookState);
            return;
        }

        moveInput = Vector2.zero;

        movement.SetMoveInput(moveInput);

    }

    public override void Exit()
    {
        base.Exit();
    }
}
