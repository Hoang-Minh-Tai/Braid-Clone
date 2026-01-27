using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlatformState
{
    public bool isOn;
    public Vector2 position;
    public float lerpValue;
    public bool isMoving;
}

public class MovingPlatform : MonoBehaviour
{
    [Header("Lever Settings")]
    [SerializeField] private Transform leverHandleTransform;
    [SerializeField] private SpriteRenderer guideSpriteRenderer;

    [Header("Points")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private Transform platformTransform;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("State")]
    [SerializeField] private bool isOn = false;

    [Header("Audio")]
    [SerializeField] private RewindableAudioPlayer leverAudioSource;
    [SerializeField] private RewindableAudioPlayer platformAudioSource;

    // Movement data
    private float lerpValue = 0f;
    private bool isMoving;
    private bool wasMoving;

    // Rewind data
    private readonly List<PlatformState> states = new();

    private RewindTimeManager rewindManager => RewindTimeManager.instance;

    void Start()
    {
        platformTransform.position = pointA.position;
        guideSpriteRenderer.enabled = false;
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
            if (states.Count == 0) return;

            int stateIndex = rewindManager.currentFrameIndex;
            stateIndex = Mathf.Clamp(stateIndex, 0, states.Count - 1);

            RestoreState(states[stateIndex]);
            return;
        }

        // -------- NORMAL MODE --------
        states.Add(RecordState());

        float target = isOn ? 1f : 0f;
        wasMoving = isMoving;

        lerpValue = Mathf.MoveTowards(
            lerpValue,
            target,
            Time.fixedDeltaTime * moveSpeed
        );

        platformTransform.position = Vector2.Lerp(
            pointA.position,
            pointB.position,
            lerpValue
        );

        isMoving = !Mathf.Approximately(lerpValue, target);

        // Platform audio control
        if (!wasMoving && isMoving)
        {
            platformAudioSource.Play("platform");
        }
        else if (wasMoving && !isMoving)
        {
            platformAudioSource.Stop();
        }
    }

    // --------------------------------------
    // INTERACTION
    // --------------------------------------

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            guideSpriteRenderer.enabled = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            guideSpriteRenderer.enabled = false;
        }
    }

    public void Toggle()
    {
        isOn = !isOn;

        leverAudioSource.Play(isOn ? "lever_on" : "lever_off");

        FlipHandle();
    }

    // --------------------------------------
    // REWIND MODE
    // --------------------------------------

    private void EnableRewindMode(int rewindSpeed)
    {
        guideSpriteRenderer.enabled = false;
    }

    private void DisableRewindMode()
    {
        int stateIndex = rewindManager.currentFrameIndex;

        if (stateIndex < states.Count - 1)
        {
            states.RemoveRange(stateIndex + 1, states.Count - (stateIndex + 1));
        }
    }

    // --------------------------------------
    // STATE RECORD / RESTORE
    // --------------------------------------

    private PlatformState RecordState()
    {
        return new PlatformState
        {
            isOn = isOn,
            position = platformTransform.position,
            lerpValue = lerpValue,
            isMoving = isMoving
        };
    }

    private void RestoreState(PlatformState state)
    {
        bool prevMoving = isMoving;

        isOn = state.isOn;
        lerpValue = state.lerpValue;
        isMoving = state.isMoving;

        platformTransform.position = state.position;
        FlipHandle();

        // Sync platform audio
        if (!prevMoving && isMoving)
        {
            platformAudioSource.Play("platform");
        }
        else if (prevMoving && !isMoving)
        {
            platformAudioSource.Stop();
        }
    }

    private void FlipHandle()
    {
        leverHandleTransform.localEulerAngles = new Vector3(
            leverHandleTransform.localEulerAngles.x,
            leverHandleTransform.localEulerAngles.y,
            isOn ? -44.35f : 44.35f
        );

        guideSpriteRenderer.flipX = isOn;
    }
}
