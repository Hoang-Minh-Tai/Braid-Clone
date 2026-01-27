using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class MobHuntIconState
{
    public Color iconColor;
    public float crossAlpha;
}

public class MobHuntIcon : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image icon;
    [SerializeField] private Image redCross;

    [Header("Settings")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color disabledColor = new Color(0.65f, 0.65f, 0.65f); // Hex A6A6A6
    [SerializeField] private float fadeSpeed = 3f;

    // Rewind
    private bool rewinding = false;
    private List<MobHuntIconState> states = new List<MobHuntIconState>();
    private int stateIndex = 0;

    private readonly int[] rewindSteps = { -8, -4, -2, -1, 0, 1, 2, 4, 8 };
    public int speedIndex = 4; // middle = no movement

    private float targetCrossAlpha = 0f;

    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(EnableRewindMode);
        events.onRewindEnd.AddListener(DisableRewindMode);
        events.onRewindBackward.AddListener(RewindBackward);
        events.onRewindForward.AddListener(RewindForward);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(EnableRewindMode);
        events.onRewindEnd.RemoveListener(DisableRewindMode);
        events.onRewindBackward.RemoveListener(RewindBackward);
        events.onRewindForward.RemoveListener(RewindForward);
    }

    private void FixedUpdate()
    {
        if (rewinding)
        {
            if (states.Count == 0) return;

            int step = rewindSteps[speedIndex];
            stateIndex += step;
            stateIndex = Mathf.Clamp(stateIndex, 0, states.Count - 1);

            RestoreState(states[stateIndex]);
            return;
        }

        // NORMAL MODE: smoothly update cross alpha
        var color = redCross.color;
        color.a = Mathf.MoveTowards(color.a, targetCrossAlpha, fadeSpeed * Time.fixedDeltaTime);
        redCross.color = color;

        // Record state
        states.Add(RecordState());
        stateIndex = states.Count - 1;
    }

    // --------------------------------------
    // ENABLE CROSS
    // --------------------------------------
    [ContextMenu("Trigger")]
    public void TriggerRedCross()
    {
        icon.color = disabledColor;
        targetCrossAlpha = 1f; // fade in to full
    }

    // --------------------------------------
    // REWIND MODE
    // --------------------------------------
    private void EnableRewindMode(int rewindSpeed)
    {
        rewinding = true;
        speedIndex = rewindSpeed == 0 ? 4 : 3;
    }

    private void DisableRewindMode()
    {
        rewinding = false;

        if (stateIndex < states.Count - 1)
            states.RemoveRange(stateIndex + 1, states.Count - (stateIndex + 1));

        // Ensure target alpha matches current state
        targetCrossAlpha = redCross.color.a;
    }

    private void RewindBackward()
    {
        if (!rewinding) return;
        speedIndex = Mathf.Clamp(speedIndex - 1, 0, rewindSteps.Length - 1);
    }

    private void RewindForward()
    {
        if (!rewinding) return;
        speedIndex = Mathf.Clamp(speedIndex + 1, 0, rewindSteps.Length - 1);
    }

    // --------------------------------------
    // STATE RECORDING / RESTORATION
    // --------------------------------------
    protected virtual MobHuntIconState RecordState()
    {
        return new MobHuntIconState
        {
            iconColor = icon.color,
            crossAlpha = redCross.color.a
        };
    }

    protected virtual void RestoreState(MobHuntIconState state)
    {
        icon.color = state.iconColor;
        var crossColor = redCross.color;
        crossColor.a = state.crossAlpha;
        redCross.color = crossColor;

        // Update target alpha so fade picks up smoothly
        targetCrossAlpha = state.crossAlpha;
    }
}
