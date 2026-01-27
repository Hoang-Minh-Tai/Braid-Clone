using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class HedgeHogTimeRecord
{
    public Sprite sprite;
    public int direction;
    public Vector2 position;
    public Vector2 velocity;
    public bool dead;
}

public class HedgeHog_TimeTrack : MonoBehaviour
{
    private List<HedgeHogTimeRecord> timeTrack = new();

    private SpriteRenderer rd;
    private Rigidbody2D rb;
    private Animator animator;
    private Mob_HedgeHog hedgeHog;

    private RewindTimeManager rewindManager => RewindTimeManager.instance;

    private void Awake()
    {
        hedgeHog = GetComponent<Mob_HedgeHog>();
        rd = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (GameEventManager.instance != null)
        {
            var events = GameEventManager.instance.generalEvent;

            events.onRewindStart.AddListener(EnableRewindMode);
            events.onRewindEnd.AddListener(DisableRewindMode);
        }
    }

    private void OnDisable()
    {
        if (GameEventManager.instance != null)
        {
            var events = GameEventManager.instance.generalEvent;

            events.onRewindStart.RemoveListener(EnableRewindMode);
            events.onRewindEnd.RemoveListener(DisableRewindMode);
        }
    }

    private void FixedUpdate()
    {
        if (rewindManager.rewinding)
        {
            if (timeTrack.Count == 0) return;

            int rewindIndex = rewindManager.currentFrameIndex;
            rewindIndex = Mathf.Clamp(rewindIndex, 0, timeTrack.Count - 1);

            LoadRecord(timeTrack[rewindIndex]);
            return;
        }

        RecordTrack();
    }

    private void RecordTrack()
    {
        var record = new HedgeHogTimeRecord
        {
            sprite = rd.sprite,
            position = transform.position,
            velocity = rb.linearVelocity,
            direction = (int)transform.localScale.x,
            dead = hedgeHog.Dead,
        };

        timeTrack.Add(record);
    }

    private void EnableRewindMode(int speed)
    {
        animator.enabled = false;
        rb.simulated = false;
    }

    private void DisableRewindMode()
    {
        rb.simulated = true;
        animator.enabled = true;
        ClearFuture();
    }

    private void LoadRecord(HedgeHogTimeRecord record)
    {
        rd.sprite = record.sprite;
        transform.position = record.position;
        rb.linearVelocity = record.velocity;
        hedgeHog.SetDirection(record.direction);
        hedgeHog.Dead = record.dead;
    }

    private void ClearFuture()
    {
        int tickIndex = rewindManager.currentFrameIndex;
        if (tickIndex < timeTrack.Count - 1)
        {
            timeTrack.RemoveRange(tickIndex + 1, timeTrack.Count - (tickIndex + 1));
        }
    }
}
