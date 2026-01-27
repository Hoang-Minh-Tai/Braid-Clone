using UnityEngine;

public class PlayerState_TimeRewind : PlayerState
{
    private Player_TimeShiftTrack timeTrack;
    private readonly int[] rewindSteps = { -8, -4, -2, -1, 0, 1, 2, 4, 8 };
    private int speedIndex;


    public PlayerState_TimeRewind(StateMachine stateMachine, string stateName = "") : base(stateMachine, stateName)
    {
        timeTrack = player.GetComponent<Player_TimeShiftTrack>();
    }

    public override void Enter()
    {
        base.Enter();

        speedIndex = 3;
        animator.enabled = false;
        movement.enabled = false;
        rb.simulated = false;
        timeTrack.EnableRewindMode();
    }

    public override void Update()
    {
        base.Update();

        if (input.Player.Rewind.WasReleasedThisFrame())
        {
            var currentTimeRecord = timeTrack.GetCurrentRecord();
            stateMachine.ChangeState(player.GetStateFromEnum(currentTimeRecord.state));

            GameEventManager.instance.generalEvent.EndRewind();
            return;
        }

        if (input.Player.RewindForward.WasPressedThisFrame())
        {
            speedIndex = Mathf.Clamp(speedIndex + 1, 0, rewindSteps.Length - 1);
            GameEventManager.instance.generalEvent.RewindForward();
        }

        if (input.Player.RewindBackward.WasPressedThisFrame())
        {
            speedIndex = Mathf.Clamp(speedIndex - 1, 0, rewindSteps.Length - 1);
            GameEventManager.instance.generalEvent.RewindBackward();
        }
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        LoadRecord();
    }

    public override void Exit()
    {
        base.Exit();
        animator.enabled = true;
        movement.enabled = true;
        rb.simulated = true;
        timeTrack.ClearFuture();
        timeTrack.DisableRewindMode();
    }

    public override void SetRewindSpeed(int speed)
    {
        if (speed == 0)
        {
            speedIndex = 4;
        }
        else
        {
            speedIndex = 3;
        }
    }

    private void LoadRecord()
    {
        PlayerTimeRecord record = timeTrack.GetRecord(rewindSteps[speedIndex]);

        rd.sprite = record.sprite;
        player.transform.position = record.position;
        rb.linearVelocity = record.velocity;
        movement.Flip(record.direction);
        stateMachine.enableMovement = record.enableMovement;
    }

}
