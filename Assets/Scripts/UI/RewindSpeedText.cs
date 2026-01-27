using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RewindSpeedTextUI : MonoBehaviour
{
    public TextMeshProUGUI rewindText;   // TMP UI
    public Image buttonsImage;           // UI button image
    public Transform player;             // Player to follow
    public Vector3 offset = new Vector3(0, 2, 0); // Offset above player

    [Header("UI Settings")]
    public float displayTime = 1f;
    public float fadeDuration = 0.5f;
    public float buttonDelay = 10f;

    [Header("Rewind Steps")]
    private readonly int[] rewindSteps = { -8, -4, -2, -1, 0, 1, 2, 4, 8 };
    public int speedIndex = 3;

    private Coroutine fadeCoroutine;
    private Coroutine buttonCoroutine;
    private Vector3 currentScreenPos;

    private void Awake()
    {
        if (rewindText == null)
            rewindText = GetComponentInChildren<TextMeshProUGUI>();

        SetTextAlpha(0f);

        if (buttonsImage != null)
            SetButtonAlpha(0f); // hide buttons initially
    }

    void Start()
    {
        player = Player.Instance.transform;
    }
    // private void Update()
    // {
    //     if (player != null && rewindText != null)
    //     {
    //         // Target screen position above the player
    //         Vector3 targetPos = Camera.main.WorldToScreenPoint(player.position + offset);

    //         // Smoothly interpolate the UI position
    //         currentScreenPos = Vector3.Lerp(currentScreenPos, targetPos, 20f * Time.deltaTime);

    //         rewindText.rectTransform.position = currentScreenPos;
    //     }
    // }

    private void LateUpdate()
    {
        if (player == null || rewindText == null || Camera.main == null)
            return;

        Vector3 screenPos = Camera.main.WorldToScreenPoint(player.position + offset);
        rewindText.rectTransform.position = screenPos;
        buttonsImage.rectTransform.position = screenPos;
    }


    private void OnEnable()
    {
        if (GameEventManager.instance != null)
        {
            var events = GameEventManager.instance.generalEvent;

            events.onRewindBackward.AddListener(() => ShowSpeedText(-1));
            events.onRewindForward.AddListener(() => ShowSpeedText(1));
            events.onRewindStart.AddListener(OnRewindStart);
            events.onRewindEnd.AddListener(OnRewindEnd);
        }
    }

    private void OnDisable()
    {
        if (GameEventManager.instance != null)
        {
            var events = GameEventManager.instance.generalEvent;

            events.onRewindBackward.RemoveAllListeners();
            events.onRewindForward.RemoveAllListeners();
            events.onRewindStart.RemoveListener(OnRewindStart);
            events.onRewindEnd.RemoveListener(OnRewindEnd);
        }
    }

    private void OnRewindStart(int rewindValue)
    {
        speedIndex = (rewindValue == 0) ? 4 : 3;

        if (buttonCoroutine != null)
            StopCoroutine(buttonCoroutine);

        buttonCoroutine = StartCoroutine(ShowButtonAfterDelay(buttonDelay));
    }

    private void OnRewindEnd()
    {
        if (buttonCoroutine != null)
            StopCoroutine(buttonCoroutine);

        if (buttonsImage != null)
            StartCoroutine(FadeButton(0f));
    }

    private void ShowSpeedText(int direction)
    {
        speedIndex = Mathf.Clamp(speedIndex + direction, 0, rewindSteps.Length - 1);
        int speed = rewindSteps[speedIndex];
        rewindText.text = "x" + speed;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        SetTextAlpha(1f);

        fadeCoroutine = StartCoroutine(FadeOutText(displayTime, fadeDuration));

        if (buttonsImage != null)
        {
            if (buttonCoroutine != null)
                StopCoroutine(buttonCoroutine);
            StartCoroutine(FadeButton(0f));
        }
    }

    private IEnumerator FadeOutText(float delay, float duration)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        Color startColor = rewindText.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            SetTextAlpha(alpha);
            yield return null;
        }

        SetTextAlpha(0f);
    }

    private void SetTextAlpha(float alpha)
    {
        if (rewindText != null)
        {
            Color c = rewindText.color;
            c.a = alpha;
            rewindText.color = c;
        }
    }

    private IEnumerator ShowButtonAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (buttonsImage != null)
            yield return FadeButton(1f);
    }

    private IEnumerator FadeButton(float targetAlpha)
    {
        if (buttonsImage == null)
            yield break;

        float elapsed = 0f;
        Color startColor = buttonsImage.color;
        Color targetColor = startColor;
        targetColor.a = targetAlpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            buttonsImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
            yield return null;
        }

        buttonsImage.color = targetColor;
    }

    private void SetButtonAlpha(float alpha)
    {
        if (buttonsImage != null)
        {
            Color c = buttonsImage.color;
            c.a = alpha;
            buttonsImage.color = c;
        }
    }
}
