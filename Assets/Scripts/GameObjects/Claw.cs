using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ClawState
{
    public int phase;
    public Vector2 bodyPos;
    public float lerpValue;
    public float timer;

    // Animator rewind data
    public int animStateHash;
    public float animNormalizedTime;
}

public class Claw : MonoBehaviour
{
    private enum Phase
    {
        Rising,
        ClawAction,
        IdleAtTop,
        Falling,
        IdleAtBottom
    }

    [Header("References")]
    [SerializeField] private Transform body;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform origin;
    [SerializeField] private Transform target;
    private RewindableAudioPlayer audioPlayer;
    private AnimationTrigger animationTrigger;
    private SpriteRenderer bodyRenderer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;

    [Header("Timing")]
    [SerializeField] private float clawAnimDuration = 1f;
    [SerializeField] private float idleTopTime = 1f;
    [SerializeField] private float idleBottomTime = 1f;

    [SerializeField]
    private Phase phase;
    private float lerpValue;
    private float timer;

    private List<ClawState> states = new();

    private RewindTimeManager rewindManager => RewindTimeManager.instance;

    private void Awake()
    {
        audioPlayer = GetComponentInChildren<RewindableAudioPlayer>();
        animationTrigger = GetComponentInChildren<AnimationTrigger>();
        bodyRenderer = body.GetComponentInChildren<SpriteRenderer>();
        if (!animator)
            animator = GetComponent<Animator>();

        if (!origin || !target)
        {
            Debug.LogError("Origin and Target transforms must be assigned.");
            enabled = false;
            return;
        }

        phase = Phase.Rising;
    }

    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(EnableRewindMode);
        events.onRewindEnd.AddListener(DisableRewindMode);
        animationTrigger.snapEvent.AddListener(PlaySnapSound);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(EnableRewindMode);
        events.onRewindEnd.RemoveListener(DisableRewindMode);
        animationTrigger.snapEvent.RemoveListener(PlaySnapSound);
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

        states.Add(RecordState());

        UpdateStateMachine();
    }

    private void PlaySnapSound()
    {
        Transform player = Player.Instance.transform;
        if (Vector2.Distance(player.position, body.position) > 10f) return;
        audioPlayer.Play("snap");
    }

    private void UpdateStateMachine()
    {
        switch (phase)
        {
            case Phase.Rising:
                lerpValue = Mathf.MoveTowards(lerpValue, 1f, Time.deltaTime * moveSpeed);
                body.position = Vector2.Lerp(origin.position, target.position, lerpValue);

                if (lerpValue >= 1f)
                {
                    phase = Phase.ClawAction;
                    timer = 0f;
                    animator.Play("Idle", 0, 0f);
                }
                break;

            case Phase.ClawAction:
                timer += Time.deltaTime;
                if (timer >= clawAnimDuration)
                {
                    phase = Phase.IdleAtTop;
                    timer = 0f;
                }
                break;

            case Phase.IdleAtTop:
                timer += Time.deltaTime;
                if (timer >= idleTopTime)
                {
                    phase = Phase.Falling;
                }
                break;

            case Phase.Falling:
                lerpValue = Mathf.MoveTowards(lerpValue, 0f, Time.deltaTime * moveSpeed);
                body.position = Vector2.Lerp(origin.position, target.position, lerpValue);

                if (lerpValue <= 0f)
                {
                    phase = Phase.IdleAtBottom;
                    timer = 0f;
                }
                break;

            case Phase.IdleAtBottom:
                timer += Time.deltaTime;
                if (timer >= idleBottomTime)
                {
                    phase = Phase.Rising;
                    animator.Play("Claw", 0, 0f);
                }
                break;
        }
    }

    private void EnableRewindMode(int rewindSpeed)
    {
        animator.speed = 0f;
    }

    private void DisableRewindMode()
    {
        animator.speed = 1f;
        int stateIndex = rewindManager.currentFrameIndex;
        if (stateIndex < states.Count - 1)
            states.RemoveRange(stateIndex + 1, states.Count - stateIndex - 1);
    }

    private ClawState RecordState()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        return new ClawState
        {
            phase = (int)phase,
            bodyPos = body.position,
            lerpValue = lerpValue,
            timer = timer,
            animStateHash = info.fullPathHash,
            animNormalizedTime = info.normalizedTime
        };
    }

    private void RestoreState(ClawState state)
    {
        phase = (Phase)state.phase;
        lerpValue = state.lerpValue;
        timer = state.timer;
        body.position = state.bodyPos;

        animator.Play(state.animStateHash, 0, state.animNormalizedTime);
        animator.Update(0f);
    }
}
