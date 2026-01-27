using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GreeterState
{
    public Vector2 position;
    public string currentAction;
    public Greeter.Phase phase;
    public int direction; // -1 for left, 1 for right
    public float timer;

    public int animStateHash;
    public float animNormalizedTime;
    public bool dialogueOn;
}

public class Greeter : MonoBehaviour
{
    public enum Phase
    {
        Idle,
        MoveToTarget,
        Talk
    }

    [SerializeField] private Animator animator;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float talkDuration = 3f;
    [SerializeField] private Transform targetPosition;
    [SerializeField] private Dialogue dialogueBox;

    private Rigidbody2D rb;
    private string currentAction = "idle";

    private Phase phase = Phase.Idle;
    private float timer;

    private List<GreeterState> states = new();
    private RewindTimeManager rewindTimeManager => RewindTimeManager.instance;

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
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
        UpdateAnimator();
        if (rewindTimeManager.rewinding)
        {
            if (states.Count == 0) return;

            int stateIndex = rewindTimeManager.currentFrameIndex;
            stateIndex = Mathf.Clamp(stateIndex, 0, states.Count - 1);

            RestoreState(states[stateIndex]);
            return;
        }

        states.Add(RecordState());

        UpdateStateMachine();
    }

    private void UpdateStateMachine()
    {
        switch (phase)
        {
            case Phase.Idle:
                currentAction = "idle";
                break;

            case Phase.MoveToTarget:
                currentAction = "walk";
                MoveTowardsTarget();
                if (Mathf.Abs(transform.position.x - targetPosition.position.x) < 0.2f)
                {
                    phase = Phase.Talk;
                    timer = 0f;
                    dialogueBox.ShowDialogue();
                }
                break;

            case Phase.Talk:
                currentAction = "talk";
                timer += Time.deltaTime;
                if (timer > talkDuration) // Example talk duration
                {
                    phase = Phase.Idle;
                }
                break;
        }

    }

    [ContextMenu("Start event")]
    public void StartEvent()
    {
        phase = Phase.MoveToTarget;
    }

    private void MoveTowardsTarget()
    {
        rb.linearVelocityX = -1 * moveSpeed;
    }

    private void UpdateAnimator()
    {
        animator.SetBool("talk", currentAction == "talk");
        animator.SetBool("idle", currentAction == "idle");
        animator.SetBool("walk", currentAction == "walk");
        animator.SetBool("walk_n_talk", currentAction == "walk_n_talk");
    }

    public void SetAction(string action, int newDirection)
    {
        currentAction = action;
    }

    private GreeterState RecordState()
    {
        AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);

        return new GreeterState
        {
            position = transform.position,
            phase = phase,
            timer = timer,
            currentAction = currentAction,
            animStateHash = info.fullPathHash,
            animNormalizedTime = info.normalizedTime,
        };
    }

    private void RestoreState(GreeterState state)
    {
        transform.position = state.position;
        currentAction = state.currentAction;
        timer = state.timer;

        // Update scale based on restored direction
        phase = state.phase;

        animator.Play(state.animStateHash, 0, state.animNormalizedTime);
        animator.Update(0f);
    }

    public void EnableRewindMode(int rewindSpeed)
    {
        animator.speed = 0f;
    }

    public void DisableRewindMode()
    {
        animator.speed = 1f;

        int stateIndex = RewindTimeManager.instance.currentFrameIndex;
        if (stateIndex < states.Count - 1)
            states.RemoveRange(stateIndex + 1, states.Count - stateIndex - 1);
    }
}
