using UnityEngine;

public class PlayerState_Climb : PlayerState
{
    public PlayerState_Climb(StateMachine stateMachine, string stateName = "Climb") : base(stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platform"), true);
        rb.gravityScale = 0;
    }

    public override void Update()
    {
        base.Update();

        movement.SetMoveInput(moveInput);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void Exit()
    {
        base.Exit();
        rb.gravityScale = 1;
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Platform"), false);
        if (!RewindTimeManager.instance.rewinding) audioPlayer.Play("stop_climbing");
    }
}
