using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GateState
{
    public bool open;
    public Vector2 bodyPos;
    public float lerpValue;
}

public class Gate : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform body;
    private RewindableAudioPlayer audioPlayer;
    private Collider2D bodyCollider;

    [Header("Gate Settings")]
    [SerializeField] private float slideRange = 1f;
    [SerializeField] private float slideSpeed = 4f;

    private bool open;
    private Vector2 closedPos;
    private Vector2 openPos;

    // Rewind data
    private List<GateState> gateStates = new List<GateState>();

    private float lerpValue = 0f;

    private RewindTimeManager rewindManager => RewindTimeManager.instance;

    private void Awake()
    {
        bodyCollider = GetComponent<Collider2D>();
        closedPos = body.localPosition;
        audioPlayer = GetComponentInChildren<RewindableAudioPlayer>();
        openPos = closedPos - new Vector2(0, slideRange);
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
        if (rewindManager.rewinding)
        {
            if (gateStates.Count == 0) return;

            // Use global RewindSpeed from manager
            int stateIndex = rewindManager.currentFrameIndex;
            stateIndex = Mathf.Clamp(stateIndex, 0, gateStates.Count - 1);

            RestoreState(gateStates[stateIndex]);
            return;
        }

        // NORMAL MODE
        gateStates.Add(RecordState());

        if (open)
            lerpValue = Mathf.MoveTowards(lerpValue, 1f, Time.deltaTime * slideSpeed);

        body.localPosition = Vector2.Lerp(closedPos, openPos, lerpValue);
    }

    [ContextMenu("Open Gate")]
    public void Open()
    {
        open = true;
        bodyCollider.enabled = false;
        lerpValue = 0;
        audioPlayer.Play("open");
    }

    // --------------------------------------
    // REWIND MODE
    // --------------------------------------

    private void EnableRewindMode(int rewindSpeed)
    {
        // No specific speed logic needed; rely on manager
    }

    private void DisableRewindMode()
    {
        bodyCollider.enabled = !open;
        int stateIndex = rewindManager.currentFrameIndex;

        // DELETE all states AFTER the current point
        if (stateIndex < gateStates.Count - 1)
            gateStates.RemoveRange(stateIndex + 1, gateStates.Count - (stateIndex + 1));
    }

    // --------------------------------------
    // STATE RECORDING / RESTORATION
    // --------------------------------------

    protected virtual GateState RecordState()
    {
        return new GateState
        {
            open = open,
            bodyPos = body.localPosition,
            lerpValue = lerpValue
        };
    }

    protected virtual void RestoreState(GateState state)
    {
        open = state.open;
        lerpValue = state.lerpValue;
        body.localPosition = state.bodyPos;
    }

    public void FillBaseState(GateState state)
    {
        state.open = open;
        state.bodyPos = body.localPosition;
        state.lerpValue = lerpValue;
    }
}
