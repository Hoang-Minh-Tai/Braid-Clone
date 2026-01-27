using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerTimeRecord
{
    public int frameIndex;
    public Sprite sprite;
    public int direction;
    public Vector2 position;
    public Vector2 velocity;
    public PlayerStateEnum state;
    public bool enableMovement;
}

public class Player_TimeShiftTrack : MonoBehaviour
{
    private List<PlayerTimeRecord> timeTrack = new();

    private SpriteRenderer rd;
    private Rigidbody2D rb;
    private StateMachine stateMachine;

    private bool rewindMode;

    private RewindTimeManager rewindManager => RewindTimeManager.instance;


    private void Awake()
    {
        rd = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        stateMachine = GetComponent<Player>().stateMachine;
    }

    private void FixedUpdate()
    {
        RecordTrack();
    }

    public PlayerTimeRecord GetCurrentRecord()
    {
        int tickIndex = rewindManager.currentFrameIndex;
        tickIndex = Mathf.Clamp(tickIndex, 0, timeTrack.Count - 1);
        return timeTrack[tickIndex];
    }

    public void RecordTrack()
    {
        if (rewindMode) return;

        var record = new PlayerTimeRecord
        {
            frameIndex = timeTrack.Count,
            sprite = rd.sprite,
            position = transform.position,
            velocity = rb.linearVelocity,
            direction = (int)transform.localScale.x,
            state = stateMachine.currentStateType,
            enableMovement = stateMachine.enableMovement
        };

        timeTrack.Add(record);
    }

    public PlayerTimeRecord GetRecord(int rewindSpeed)
    {
        int currentFrame = Mathf.Clamp(rewindManager.currentFrameIndex, 0, timeTrack.Count - 1);

        return timeTrack[currentFrame];
    }

    public void ClearFuture()
    {
        int currentFrame = Mathf.Clamp(rewindManager.currentFrameIndex, 0, timeTrack.Count - 1);
        if (currentFrame < timeTrack.Count - 1)
        {
            timeTrack.RemoveRange(currentFrame + 1, timeTrack.Count - (currentFrame + 1));
        }
    }

    public void DisableMovement()
    {
        stateMachine.enableMovement = false;
    }

    public void EnableRewindMode()
    {
        rewindMode = true;
    }

    public void DisableRewindMode()
    {
        rewindMode = false;
    }

    public List<PlayerTimeRecord> GetTimeTrack()
    {
        return timeTrack;
    }
}
