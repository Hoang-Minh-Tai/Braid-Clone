using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KeyState
{
    public Vector2 position;
    public bool used;
    public bool flipX;
}

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Key : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] private SpriteRenderer spriteRenderer;
    private RewindableAudioPlayer audioPlayer;
    [SerializeField] private Sprite lockSprite;
    [SerializeField] private Sprite unlockSprite;

    [Header("Attach")]
    public Transform attachPoint;
    private RewindTimeManager rewindTimeManager => RewindTimeManager.instance;

    private Rigidbody2D rb;
    private bool used;

    // -------------------------
    // REWIND DATA
    // -------------------------
    private bool rewinding;
    private readonly List<KeyState> states = new();


    // -------------------------
    // UNITY
    // -------------------------
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioPlayer = GetComponentInChildren<RewindableAudioPlayer>();
    }

    private void OnEnable()
    {
        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(EnableRewind);
        events.onRewindEnd.AddListener(DisableRewind);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(EnableRewind);
        events.onRewindEnd.RemoveListener(DisableRewind);
    }

    private void FixedUpdate()
    {
        // -------------------------
        // REWIND MODE
        // -------------------------
        if (rewindTimeManager.rewinding)
        {
            if (states.Count == 0) return;

            int stateIndex = rewindTimeManager.currentFrameIndex;
            stateIndex = Mathf.Clamp(stateIndex, 0, states.Count - 1);
            RestoreState(states[stateIndex]);
            return;
        }

        // -------------------------
        // NORMAL MODE
        // -------------------------
        states.Add(RecordState());
    }

    private void LateUpdate()
    {
        // Only follow holder facing during normal time
        if (rewinding) return;
    }

    // -------------------------
    // TRIGGERS
    // -------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (rewinding || used || attachPoint != null) return;

        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<Player>();
            if (player != null)
                AttachTo(player.keyAttachPoint);
        }
        else if (other.CompareTag("Enemy"))
        {
            var enemy = other.GetComponent<Mob_HedgeHog>();
            if (enemy != null)
                AttachTo(enemy.keyAttachPoint);
        }
    }

    public void UseKey()
    {
        if (used) return;

        used = true;
        Detach();
        spriteRenderer.sprite = unlockSprite;
        audioPlayer.Play("key_use");
    }

    // -------------------------
    // ATTACH / DETACH
    // -------------------------
    private void AttachTo(Transform point)
    {
        attachPoint = point;

        transform.SetParent(attachPoint, false);
        transform.localPosition = Vector3.zero;
        spriteRenderer.flipX = false;

        SetAttached(true);
    }

    private void Detach()
    {
        transform.SetParent(null);
        attachPoint = null;
        spriteRenderer.flipX = false;

        SetAttached(false);
    }

    private void SetAttached(bool attached)
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.localScale = Vector3.one;
        rb.simulated = !attached;
        rb.gravityScale = attached ? 0f : 1f;
    }

    // -------------------------
    // REWIND CONTROL
    // -------------------------
    private void EnableRewind(int rewindSpeed)
    {
        rewinding = true;
    }

    private void DisableRewind()
    {
        rewinding = false;
        int stateIndex = RewindTimeManager.instance.currentFrameIndex;
        if (stateIndex < states.Count - 1)
            states.RemoveRange(stateIndex + 1, states.Count - stateIndex - 1);

        ReevaluateAttachment();
    }

    // -------------------------
    // STATE RECORD / RESTORE
    // -------------------------
    private KeyState RecordState()
    {
        return new KeyState
        {
            position = transform.position,
            used = used,
            flipX = transform.lossyScale.x < 0
        };
    }

    private void RestoreState(KeyState state)
    {
        SetUsedState(state.used);

        transform.SetParent(null);
        transform.position = state.position;

        attachPoint = null;
        SetAttached(false);

        spriteRenderer.flipX = state.flipX;
        spriteRenderer.sprite = used ? unlockSprite : lockSprite;
    }

    private void SetUsedState(bool used)
    {
        this.used = used;
        spriteRenderer.sprite = used ? unlockSprite : lockSprite;
    }

    // -------------------------
    // POST-REWIND CONTEXT ATTACH
    // -------------------------
    private void ReevaluateAttachment()
    {
        if (used) return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(
            transform.position,
            0.12f
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                AttachTo(hit.GetComponent<Player>().keyAttachPoint);
                return;
            }

            if (hit.CompareTag("Enemy"))
            {
                AttachTo(hit.GetComponent<Mob_HedgeHog>().keyAttachPoint);
                return;
            }
        }

        Detach();
    }
}
