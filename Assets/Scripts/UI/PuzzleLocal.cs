using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PuzzleState
{
    public float lerpValue;
    public float pulseDir;   // +1 or -1
    public bool mainEnabled;
    public bool collected;
}

public class PuzzleLocal : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteRenderer border;
    [SerializeField] public SpriteRenderer main;
    [SerializeField] private RewindableAudioPlayer audioPlayer;

    [Header("Settings")]
    [SerializeField] private Color targetBorderColor = Color.white;
    [SerializeField] private float pulseSpeed = 1.0f; // units per second (lerp units)

    [Header("Puzzle Info")]
    public int puzzleIndex;



    // Animation
    private bool collected = false;
    private float lerpValue = 0f;      // 0..1
    private float pulseDir = 1f;       // +1 moving toward 1, -1 moving toward 0

    private List<PuzzleState> states = new List<PuzzleState>();


    private RewindTimeManager rewindTimeManager => RewindTimeManager.instance;

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
        if (rewindTimeManager.rewinding)
        {
            if (states.Count == 0) return;

            int stateIndex = rewindTimeManager.currentFrameIndex;
            stateIndex = Mathf.Clamp(stateIndex, 0, states.Count - 1);

            RestoreState(states[stateIndex]);
            return;
        }

        // NORMAL MODE: advance lerpValue deterministically (not based on Time.time)
        if (!collected)
        {
            lerpValue += pulseDir * pulseSpeed * Time.fixedDeltaTime;

            if (lerpValue >= 1f)
            {
                lerpValue = 1f;
                pulseDir = -1f;
            }
            else if (lerpValue <= 0f)
            {
                lerpValue = 0f;
                pulseDir = 1f;
            }

            border.color = Color.Lerp(Color.black, targetBorderColor, lerpValue);
        }

        // Record state every FixedUpdate
        states.Add(RecordState());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;

        if (other.CompareTag("Player"))
        {
            collected = true;
            audioPlayer.Play("collect");
            //main.enabled = false;

            // set final visual state (optional choice)
            lerpValue = 0f;
            pulseDir = 1f;
            border.color = Color.black;

            Debug.Log($"Puzzle {puzzleIndex} collected by player!");
            GameEventManager.instance.generalEvent.PuzzleCollect(this);
        }
    }

    // --------------------------------------
    // REWIND MODE
    // --------------------------------------

    private void EnableRewindMode(int rewindSpeed)
    {
    }

    private void DisableRewindMode()
    {
        int stateIndex = rewindTimeManager.currentFrameIndex;
        if (stateIndex < states.Count - 1)
            states.RemoveRange(stateIndex + 1, states.Count - (stateIndex + 1));
    }

    // --------------------------------------
    // STATE RECORDING / RESTORATION
    // --------------------------------------

    protected virtual PuzzleState RecordState()
    {
        return new PuzzleState
        {
            lerpValue = lerpValue,
            pulseDir = pulseDir,
            mainEnabled = main.enabled,
            collected = collected
        };
    }

    protected virtual void RestoreState(PuzzleState state)
    {
        lerpValue = state.lerpValue;
        pulseDir = state.pulseDir;
        main.enabled = state.mainEnabled;
        collected = state.collected;

        // Recompute border color from lerpValue
        border.color = Color.Lerp(Color.black, targetBorderColor, lerpValue);
    }

    public void FillBaseState(PuzzleState state)
    {
        state.lerpValue = lerpValue;
        state.pulseDir = pulseDir;
        state.mainEnabled = main.enabled;
        state.collected = collected;
    }
}
