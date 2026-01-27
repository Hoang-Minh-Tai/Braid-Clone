using UnityEngine;

[ExecuteAlways]
public class RenderImageCamera : MonoBehaviour
{

    [Header("Rewind Effect Settings")]
    public Material rewind_material;
    [SerializeField] private float zoom = 4f;
    [SerializeField] private float baseStrength = 0.1875f;
    [SerializeField] private float timeSpeed = 1f;
    [SerializeField] private float strengthSmoothSpeed = 5f; // How fast it interpolates

    [Space(10)]
    [Header("Fade Settings")]
    [SerializeField] private Material fade_material;
    [SerializeField] private float fadeDuration = 1f; // Changed from fadeSpeed to fadeDuration
    [SerializeField] private float fadeRadius = 0f;
    [SerializeField] private float fadeMax = 1.5f;

    private int speedIndex = 0;
    private float currentStrength = 0f; // current value used in material

    private void Start()
    {
        ApplyMaterialProperties();
        currentStrength = baseStrength * speedIndex;

        // Start the fade effect
        StartCoroutine(FadeEffect());
    }

    private System.Collections.IEnumerator FadeEffect()
    {
        if (fade_material == null) yield break;

        float targetRadius = fadeMax; // Maximum radius value
        fadeRadius = 0f; // Start from 0
        float elapsedTime = 0f; // Track elapsed time

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeRadius = Mathf.Lerp(0f, targetRadius, elapsedTime / fadeDuration);
            fade_material.SetFloat("_Radius", fadeRadius);
            yield return null;
        }

        // Ensure the final value is set
        fade_material.SetFloat("_Radius", targetRadius);
    }

    private void OnValidate()
    {
        ApplyMaterialProperties();
    }

    private void ApplyMaterialProperties()
    {
        if (rewind_material == null) return;

        rewind_material.SetFloat("_Zoom", zoom);
        rewind_material.SetFloat("_TimeSpeed", timeSpeed);
    }

    private void Update()
    {
        SmoothStrengthUpdate();
    }

    private void SmoothStrengthUpdate()
    {
        if (rewind_material == null) return;

        float targetStrength = baseStrength * speedIndex;
        currentStrength = Mathf.Lerp(currentStrength, targetStrength, strengthSmoothSpeed * Time.deltaTime);
        rewind_material.SetFloat("_Strength", currentStrength);
    }

    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var e = GameEventManager.instance.generalEvent;
        e.onRewindStart.AddListener(OnRewindStart);
        e.onRewindEnd.AddListener(OnRewindEnd);
        e.onRewindBackward.AddListener(OnRewindBackward);
        e.onRewindForward.AddListener(OnRewindForward);

        // Listen for the open door event
        e.onOpenDoor.AddListener(OnOpenDoor);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var e = GameEventManager.instance.generalEvent;
        e.onRewindStart.RemoveListener(OnRewindStart);
        e.onRewindEnd.RemoveListener(OnRewindEnd);
        e.onRewindBackward.RemoveListener(OnRewindBackward);
        e.onRewindForward.RemoveListener(OnRewindForward);

        // Remove the open door event listener
        e.onOpenDoor.RemoveListener(OnOpenDoor);
    }
    // -----------------------------------------------------------
    // REWIND EVENTS
    // -----------------------------------------------------------

    private void OnRewindStart(int rewindSpeed)
    {
        speedIndex = rewindSpeed;
    }

    private void OnRewindEnd()
    {
        speedIndex = 0; // center
    }

    private void OnRewindBackward()
    {
        speedIndex = Mathf.Clamp(speedIndex - 1, -4, 4);
    }

    private void OnRewindForward()
    {
        speedIndex = Mathf.Clamp(speedIndex + 1, -4, 4);
    }

    private void OnOpenDoor()
    {
        // Start the fade-out effect
        StartCoroutine(FadeOutEffect());
    }

    private System.Collections.IEnumerator FadeOutEffect()
    {
        if (fade_material == null) yield break;

        float targetRadius = 0f; // Minimum radius value
        fadeRadius = fadeMax; // Start from the maximum value
        float elapsedTime = 0f; // Track elapsed time

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            fadeRadius = Mathf.Lerp(fadeMax, targetRadius, elapsedTime / fadeDuration);
            fade_material.SetFloat("_Radius", fadeRadius);
            yield return null;
        }

        // Ensure the final value is set
        fade_material.SetFloat("_Radius", targetRadius);
    }
}
