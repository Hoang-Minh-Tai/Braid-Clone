using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class FlagState
{
    public bool lowered;
    public Vector2 flagPos;
    public float lerpValue;

    // Animator rewind data
    public int animStateHash;
    public float animNormalizedTime;
}

public class Flag : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform flagMain;
    [SerializeField] private Animator animator;

    [Header("Flag Settings")]
    [SerializeField] private float dropDistance = 1.2f;
    [SerializeField] private float dropSpeed = 2f;

    private Vector2 startPos;
    private Vector2 endPos;

    private bool lowered;
    private float lerpValue;

    public UnityEvent onFlagLowered;

    // ----------------------------
    // REWIND DATA
    // ----------------------------
    private List<FlagState> flagStates = new();

    private void Awake()
    {
        startPos = flagMain.localPosition;
        endPos = startPos - new Vector2(0, dropDistance);

        if (!animator)
            animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(EnableRewindMode);
        events.onRewindEnd.AddListener(DisableRewindMode);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(EnableRewindMode);
        events.onRewindEnd.RemoveListener(DisableRewindMode);
    }

    private void FixedUpdate()
    {
        if (RewindTimeManager.instance.rewinding)
        {
            if (flagStates.Count == 0) return;

            // Use global RewindSpeed from manager
            int stateIndex = RewindTimeManager.instance.currentFrameIndex;
            stateIndex = Mathf.Clamp(stateIndex, 0, flagStates.Count - 1);

            RestoreState(flagStates[stateIndex]);
            return;
        }

        // ----------------------------
        // NORMAL MODE
        // ----------------------------
        flagStates.Add(RecordState());

        if (lowered)
            lerpValue = Mathf.MoveTowards(lerpValue, 1f, Time.deltaTime * dropSpeed);

        flagMain.localPosition = Vector2.Lerp(startPos, endPos, lerpValue);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        if (lowered) return;

        onFlagLowered?.Invoke();

        lowered = true;
        Debug.Log("Flag lowered!");
    }

    // ----------------------------
    // REWIND CONTROL
    // ----------------------------
    private void EnableRewindMode(int rewindSpeed)
    {
        // Freeze animator — we drive it manually
        animator.speed = 0f;
    }

    private void DisableRewindMode()
    {
        animator.speed = 1f;
        int stateIndex = RewindTimeManager.instance.currentFrameIndex;

        // Delete future timeline
        if (stateIndex < flagStates.Count - 1)
            flagStates.RemoveRange(stateIndex + 1, flagStates.Count - stateIndex - 1);
    }

    // ----------------------------
    // STATE SYSTEM
    // ----------------------------
    private FlagState RecordState()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        return new FlagState
        {
            lowered = lowered,
            flagPos = flagMain.localPosition,
            lerpValue = lerpValue,
            animStateHash = info.fullPathHash,
            animNormalizedTime = info.normalizedTime
        };
    }

    private void RestoreState(FlagState state)
    {
        lowered = state.lowered;
        lerpValue = state.lerpValue;
        flagMain.localPosition = state.flagPos;

        animator.Play(state.animStateHash, 0, state.animNormalizedTime);
        animator.Update(0f); // Apply pose immediately
    }
}
