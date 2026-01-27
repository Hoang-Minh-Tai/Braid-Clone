using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class RewindTimeManager : MonoBehaviour
{
    public static RewindTimeManager instance;

    [Header("Rewind Speeds")]
    [SerializeField]
    private int[] rewindSteps = { -8, -4, -2, -1, 0, 1, 2, 4, 8 };

    [SerializeField]
    private int defaultSpeedIndex = 5;

    [SerializeField]
    private int speedIndex;

    public bool rewinding;

    public int RewindSpeed => rewindSteps[speedIndex];
    public bool ReachStart => currentFrameIndex <= 0;
    public bool ReachEnd => currentFrameIndex >= totalFrames;

    [Header("Timeline")]
    public int currentFrameIndex;
    public int totalFrames;

    [Header("Events")]
    public UnityEvent<int> onSpeedChange;

    // ----------------------------
    // UNITY LIFECYCLE
    // ----------------------------
    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        SetSpeedIndex(defaultSpeedIndex);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(StartRewind);
        events.onRewindEnd.AddListener(StopRewind);
        events.onRewindBackward.AddListener(RewindSlower);
        events.onRewindForward.AddListener(RewindFaster);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(StartRewind);
        events.onRewindEnd.RemoveListener(StopRewind);
        events.onRewindBackward.RemoveListener(RewindSlower);
        events.onRewindForward.RemoveListener(RewindFaster);
    }

    private void FixedUpdate()
    {
        if (rewinding)
        {
            currentFrameIndex += RewindSpeed;
            currentFrameIndex = Mathf.Clamp(currentFrameIndex, 0, totalFrames);
        }
        else
        {
            currentFrameIndex++;
            totalFrames = currentFrameIndex;
        }
    }

    // ----------------------------
    // SCENE RESET
    // ----------------------------
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        rewinding = false;
        SetSpeedIndex(defaultSpeedIndex);

        currentFrameIndex = 0;
        totalFrames = 0;

        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindBackward.RemoveListener(RewindSlower);
        events.onRewindForward.RemoveListener(RewindFaster);
        events.onRewindBackward.AddListener(RewindSlower);
        events.onRewindForward.AddListener(RewindFaster);
    }

    // ----------------------------
    // SPEED CONTROL (SINGLE SOURCE)
    // ----------------------------
    private void SetSpeedIndex(int newIndex)
    {
        newIndex = Mathf.Clamp(newIndex, 0, rewindSteps.Length - 1);

        if (speedIndex == newIndex)
            return;

        speedIndex = newIndex;
        onSpeedChange?.Invoke(rewindSteps[newIndex]);
    }

    // ----------------------------
    // REWIND CONTROL
    // ----------------------------
    public void StartRewind(int rewindSpeed = 0)
    {
        rewinding = true;
        SetSpeedIndex(rewindSpeed == 0 ? 4 : 3);
    }

    public void StopRewind()
    {
        rewinding = false;
        SetSpeedIndex(defaultSpeedIndex);
    }

    public void RewindSlower()
    {
        if (!rewinding) return;
        SetSpeedIndex(speedIndex - 1);
    }

    public void RewindFaster()
    {
        if (!rewinding) return;
        SetSpeedIndex(speedIndex + 1);
    }
}
