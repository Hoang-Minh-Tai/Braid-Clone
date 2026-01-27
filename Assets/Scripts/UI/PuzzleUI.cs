using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PuzzleUIAnimationState
{
    public Vector3 startPos;
    public Vector3 scale;
    public float t;
    public int phase;      // 0=idle, 1=zoom, 2=fly, 3=done
    public bool active;
    public float p; // progress
}

public class PuzzleUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image worldImage;
    [SerializeField] private RectTransform uiRoot;
    [SerializeField] private Camera uiCamera;

    [Header("Settings")]
    [SerializeField] private float appearTime = 0.4f;
    [SerializeField] private float flyToUITime = 0.6f;
    [SerializeField] private AnimationCurve moveCurve;
    [SerializeField] private int puzzleIndex;

    [Header("Scale")]
    [SerializeField] private Vector3 startScale;
    [SerializeField] private Vector3 peakScale;
    [SerializeField] private Vector3 endScale;

    private Camera mainCamera;
    private Transform worldStartPos;

    private bool animating;
    private int phase;
    private float t;
    private float p;

    // rewind
    private readonly List<PuzzleUIAnimationState> states = new();
    private RewindTimeManager rewindTimeManager => RewindTimeManager.instance;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // ------------------------------------------------------------
    // LIFECYCLE
    // ------------------------------------------------------------
    private void OnEnable()
    {
        var e = GameEventManager.instance.generalEvent;
        e.onPuzzleCollect.AddListener(StartPuzzleAnimation);
        e.onRewindStart.AddListener(OnRewindStart);
        e.onRewindEnd.AddListener(OnRewindEnd);
    }

    private void OnDisable()
    {
        var e = GameEventManager.instance.generalEvent;
        e.onPuzzleCollect.RemoveListener(StartPuzzleAnimation);
        e.onRewindStart.RemoveListener(OnRewindStart);
        e.onRewindEnd.RemoveListener(OnRewindEnd);
    }

    // ------------------------------------------------------------
    // START
    // ------------------------------------------------------------
    private void StartPuzzleAnimation(PuzzleLocal puzzle)
    {
        if (puzzle.puzzleIndex != puzzleIndex)
            return;

        worldStartPos = puzzle.transform;

        worldImage.sprite = puzzle.main.sprite;
        worldImage.enabled = true;
        worldImage.gameObject.SetActive(true);

        puzzle.main.enabled = false;

        RectTransform rt = worldImage.rectTransform;
        rt.localScale = startScale;
        rt.anchoredPosition = GetStartLocalPoint();

        phase = 1;
        p = 0f;
        t = 0f;
        animating = true;
    }

    // ------------------------------------------------------------
    // UPDATE (NOT FixedUpdate)
    // ------------------------------------------------------------
    private void Update()
    {
        if (rewindTimeManager.rewinding)
        {
            return;
        }

        if (animating)
            RunAnimation();
    }

    void FixedUpdate()
    {
        if (rewindTimeManager.rewinding)
        {
            RestoreFromRewind();
            return;
        }
        states.Add(Record());
    }
    // ------------------------------------------------------------
    // ANIMATION
    // ------------------------------------------------------------
    private void RunAnimation()
    {
        t += Time.deltaTime;
        RectTransform rt = worldImage.rectTransform;

        if (phase == 1) // zoom (locked to world)
        {
            rt.anchoredPosition = GetStartLocalPoint();

            float p1 = Mathf.Clamp01(t / appearTime);
            rt.localScale = Vector3.Lerp(startScale, peakScale, p1);

            if (p1 >= 1f)
            {
                phase = 2;
                t = 0f;
            }
        }
        else if (phase == 2) // fly to slot
        {
            p = moveCurve.Evaluate(Mathf.Clamp01(t / flyToUITime));

            Vector2 startLocalPoint = GetStartLocalPoint();
            rt.anchoredPosition = Vector2.Lerp(startLocalPoint, Vector2.zero, p);
            rt.localScale = Vector3.Lerp(peakScale, endScale, p);

            if (p >= 1f)
            {
                phase = 3;
                animating = false;
            }
        }
    }

    // ------------------------------------------------------------
    // WORLD → UI
    // ------------------------------------------------------------
    private Vector2 GetStartLocalPoint()
    {
        Vector2 screenPoint = mainCamera.WorldToScreenPoint(worldStartPos.position);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            worldImage.rectTransform.parent as RectTransform,
            screenPoint,
            uiCamera,
            out Vector2 uiPos
        );

        return uiPos;
    }

    // ------------------------------------------------------------
    // REWIND
    // ------------------------------------------------------------
    private PuzzleUIAnimationState Record()
    {
        RectTransform rt = worldImage.rectTransform;

        return new PuzzleUIAnimationState
        {
            startPos = worldStartPos != null ? worldStartPos.position : Vector3.zero,
            scale = rt.localScale,
            t = t,
            p = Mathf.Clamp01(p),
            phase = phase,
            active = worldImage.enabled
        };
    }

    private void RestoreFromRewind()
    {
        if (states.Count == 0)
            return;

        int index = Mathf.Clamp(
            rewindTimeManager.currentFrameIndex,
            0,
            states.Count - 1
        );

        Restore(states[index]);
    }

    private void Restore(PuzzleUIAnimationState s)
    {
        RectTransform rt = worldImage.rectTransform;

        rt.localScale = s.scale;

        t = s.t;
        p = s.p;
        phase = s.phase;

        if (phase == 1)
        {
            rt.anchoredPosition = GetStartLocalPoint();
        }
        else if (phase == 2)
        {
            Vector2 startLocalPoint = GetStartLocalPoint();
            rt.anchoredPosition = Vector2.Lerp(startLocalPoint, Vector2.zero, p);
            rt.localScale = Vector3.Lerp(peakScale, endScale, p);
        }
        else if (phase == 3)
        {
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = endScale;
        }
        animating = s.active;
        worldImage.enabled = s.active;
    }

    private void OnRewindStart(int speed) { }

    private void OnRewindEnd()
    {
        int index = rewindTimeManager.currentFrameIndex;
        if (index < states.Count - 1)
            states.RemoveRange(index + 1, states.Count - index - 1);
    }

    // ------------------------------------------------------------
    // Debug
    // ------------------------------------------------------------
    [ContextMenu("Print start UI position")]
    public void PrintStartUIPosition()
    {
        Vector2 puzzleWorldStartPos = worldStartPos.position;
        Debug.Log($"Puzzle world start pos: {puzzleWorldStartPos}");

        Vector2 mainCameraPos = mainCamera.transform.position;
        Debug.Log($"Main camera pos: {mainCameraPos}");

        Vector2 screenPoint = mainCamera.WorldToScreenPoint(puzzleWorldStartPos);
        Debug.Log($"Puzzle screen point: {screenPoint}");

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            uiRoot,
            screenPoint,
            uiCamera,
            out Vector2 uiPos
        );
        Debug.Log($"Puzzle UI pos: {uiPos}");
    }
}
