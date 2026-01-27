using UnityEngine;

public class PlayerState_Ground : PlayerState
{
    private const float allowLadderDistance = 0.1f;

    public PlayerState_Ground(StateMachine stateMachine, string stateName) : base(stateMachine, stateName)
    {
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Update()
    {
        base.Update();

        // Check if the player is near a net and move toward it
        if (player.currentNet != null && moveInput.y > 0)
        {
            float directionToNet = player.currentNet.transform.position.x - player.transform.position.x > 0 ? 1 : -1;
            float distanceToNet = Mathf.Abs(player.currentNet.transform.position.x - player.transform.position.x);

            if (!player.currentNet.inRange)
            {
                moveInput = new Vector2(directionToNet, distanceToNet);
            }
            else
            {
                // player.transform.position = new Vector3(player.currentLadder.transform.position.x, player.transform.position.y, player.transform.position.z);
                stateMachine.ChangeState(player.climbNetState);
                audioPlayer.Play("start_climbing");
                return;
            }
        }

        if (moveInput.y > 0 && moveInput.x == 0 && player.currentLadder != null)
        {
            float directionToLadder = player.currentLadder.transform.position.x - player.transform.position.x > 0 ? 1 : -1;
            float distanceToLadder = Mathf.Abs(player.currentLadder.transform.position.x - player.transform.position.x);

            if (distanceToLadder > allowLadderDistance)
            {
                moveInput = new Vector2(directionToLadder, distanceToLadder);
            }
            else
            {
                stateMachine.ChangeState(player.climbLadderState);
                audioPlayer.Play("start_climbing");
                return;
            }
        }

        else if (moveInput.y < 0 && moveInput.x == 0 && player.currentLadder != null && movement.IsOnPlatform)
        {
            float directionToLadder = player.currentLadder.transform.position.x - player.transform.position.x > 0 ? 1 : -1;
            float distanceToLadder = Mathf.Abs(player.currentLadder.transform.position.x - player.transform.position.x);

            if (distanceToLadder > allowLadderDistance)
            {
                moveInput = new Vector2(directionToLadder, distanceToLadder);
            }
            else
            {
                if (player.transform.position.y > player.currentLadder.bottomPoint.transform.position.y + 1) stateMachine.ChangeState(player.climbLadderState);
                return;
            }
        }

        if (input.Player.Jump.WasPressedThisFrame() && stateMachine.enableMovement)
        {
            movement.Jump();
            audioPlayer.Play("jump");
            stateMachine.ChangeState(player.jumpState);
            return;
        }

        if (input.Player.Interact.WasPressedThisFrame())
        {
            audioPlayer.Play("test");
            if (player.currentDoor != null)
            {
                rb.linearVelocity = Vector2.zero;
                movement.enabled = false;
                Exit();
                animator.SetTrigger("enterDoor");
                audioPlayer.Play("door");
                GameEventManager.instance.generalEvent.OpenDoor();
                stateMachine.enable = false;
                return;
            }

            if (player.currentPlatform != null)
            {
                Debug.Log("Toggling platform");
                player.currentPlatform.Toggle();
            }
        }

        if (!movement.IsGrounded)
        {
            stateMachine.ChangeState(player.jumpState);
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
