using UnityEngine;

public class PlayerState_Dead : PlayerState
{
    private Collider2D playerCollider;
    private Camera mainCamera;

    public PlayerState_Dead(StateMachine stateMachine, string stateName = "Dead") : base(stateMachine, stateName)
    {
        playerCollider = player.GetComponent<Collider2D>();
        mainCamera = Camera.main;
        type = PlayerStateEnum.Dead;
    }

    public override void Enter()
    {
        base.Enter();
        rb.linearVelocityX = 0;
        rb.linearVelocityY = 0;
        movement.enabled = false;
        playerCollider.enabled = false;
    }

    public override void Update()
    {
        base.Update();

        if (HitScreenBottom())
        {
            GameEventManager.instance.generalEvent.StartRewind(0);
            GameEventManager.instance.generalEvent.DeadHitBottom();
            return;
        }
    }

    public override void Exit()
    {
        base.Exit();
        playerCollider.enabled = true;
    }

    private bool HitScreenBottom()
    {
        if (mainCamera == null || playerCollider == null) return false;

        // Get bottom of camera in world coordinates
        float cameraBottomY = mainCamera.transform.position.y - mainCamera.orthographicSize;

        // Get bottom of player
        float playerBottomY = player.transform.position.y + playerCollider.offset.y - (playerCollider.bounds.size.y / 2f);

        // Return true if player's bottom is below camera bottom
        return playerBottomY <= cameraBottomY;
    }

}
