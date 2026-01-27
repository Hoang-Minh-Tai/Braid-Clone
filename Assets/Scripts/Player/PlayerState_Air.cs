using UnityEngine;

public class PlayerState_Air : PlayerState
{
    private float enterTime;

    public PlayerState_Air(StateMachine stateMachine, string stateName = "") : base(stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enterTime = Time.time;
    }

    public override void Update()
    {
        base.Update();

        movement.SetMoveInput(moveInput);
        movement.Flip(movement.MoveInput.x);

        if (!movement.IsGrounded && moveInput.y > 0)
        {
            if (player.currentNet != null && Time.time - enterTime > 0.2f)
            {
                stateMachine.ChangeState(player.climbNetState);
                audioPlayer.Play("start_climbing");
                return;
            }

            if (player.currentLadder != null && Mathf.Abs(player.transform.position.x - player.currentLadder.transform.position.x) < 0.1f && Time.time - enterTime > 0.2f)
            {
                player.transform.position = new Vector3(player.currentLadder.transform.position.x, player.transform.position.y, player.transform.position.z);
                stateMachine.ChangeState(player.climbLadderState);
                audioPlayer.Play("start_climbing");
                return;
            }
        }


        if (player.landOnMob)
        {
            player.landOnMob = false;
            //movement.MobJump();
            return;
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
        // Apply air control logic if needed
    }

    public override void Exit()
    {
        base.Exit();
        // Additional logic for exiting the air state can be added here
    }
}
