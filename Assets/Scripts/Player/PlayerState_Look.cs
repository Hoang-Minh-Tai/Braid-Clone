using UnityEngine;

public class PlayerState_Look : PlayerState_Ground
{   
    public PlayerState_Look(StateMachine stateMachine) : base(stateMachine, "Look") {
        type = PlayerStateEnum.Look;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        float lookY = input.Player.Look.ReadValue<float>();

        if (lookY == 0)
        {
            stateMachine.ChangeState(player.idleState);
            return;
        }

        animator.SetFloat("lookY", lookY);

    }

    public override void Exit()
    {
        base.Exit();
    }
}
