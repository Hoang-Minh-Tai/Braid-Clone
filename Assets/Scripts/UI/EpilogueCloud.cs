using UnityEngine;
using System.Collections.Generic;

public class EpilogueCloud : MonoBehaviour
{
    // General Settings
    [SerializeField] private float speed = 0.1f;
    [SerializeField] private Vector2 direction = Vector2.up;

    // Fade Settings
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 2f;
    [SerializeField] private float minWaitTime = 1f;
    [SerializeField] private float maxWaitTime = 5f;
    [SerializeField] private bool enableFadeMode = true; // Option to enable or disable fade mode

    // Movement Settings
    [Header("Movement Settings")]
    [SerializeField] private float moveDuration = 5f; // Duration before fading starts
    [SerializeField] private bool enableRotation = false; // Option to enable rotation
    [SerializeField] private float rotationSpeed = 5f; // Rotation speed in degrees per second
    [SerializeField] private bool rotateClockwise = true; // Option to rotate clockwise or counterclockwise

    // Fluctuation Settings
    [Header("Fluctuation Settings")]
    [SerializeField] private float fluctuationSpeed = 1f; // Speed of alpha fluctuation
    [SerializeField] private float fluctuationDuration = 5f; // Duration of fluctuation before disappearing
    [SerializeField] private float minFluctuationDuration = 3f; // Minimum fluctuation duration
    [SerializeField] private float maxFluctuationDuration = 7f; // Maximum fluctuation duration
    [SerializeField] private float minAlpha = 0.1f; // Minimum alpha value during fluctuation
    [SerializeField] private float maxAlpha = 1f; // Maximum alpha value during fluctuation

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 startPosition;

    private bool isFading;
    private float fadeTimer;
    private float waitTimer;
    private bool isWaiting;
    private float moveTimer;

    // Rewind data
    private List<CloudState> cloudStates = new();

    private bool isFadingIn;

    private float currentRotation; // Track the current rotation angle

    // Fluctuation variables
    private float fluctuationTimer;
    private float targetAlpha; // Target alpha value for smooth transitions

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Random rotation at start
        rotateClockwise = Random.value > 0.5f;

        if (spriteRenderer == null)
        {
            Debug.LogError("EpilogueCloud requires a SpriteRenderer component.");
            enabled = false;
            return;
        }

        originalColor = spriteRenderer.color;
        startPosition = transform.position;
        targetAlpha = 1f;
        ResetCloud();
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
    }

    private void FixedUpdate()
    {
        if (RewindTimeManager.instance.rewinding)
        {
            if (cloudStates.Count == 0) return;

            int stateIndex = RewindTimeManager.instance.currentFrameIndex;
            stateIndex = Mathf.Clamp(stateIndex, 0, cloudStates.Count - 1);

            RestoreState(cloudStates[stateIndex]);
            return;
        }

        // Record state in normal mode
        cloudStates.Add(RecordState());

        if (isWaiting)
        {
            waitTimer -= Time.fixedDeltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                ResetCloud();
            }
            return;
        }

        // Rotate the cloud if enabled
        if (enableRotation)
        {
            float rotationDirection = rotateClockwise ? 1f : -1f;
            currentRotation += rotationDirection * rotationSpeed * Time.fixedDeltaTime;
            transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
        }

        // Move the cloud
        transform.Translate(direction * speed * Time.fixedDeltaTime);

        // Smoothly transition alpha toward the target alpha
        Color currentColor = spriteRenderer.color;
        float newAlpha = Mathf.MoveTowards(currentColor.a, targetAlpha, Time.fixedDeltaTime / fadeDuration);
        spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

        if (isFadingIn)
        {
            if (Mathf.Approximately(newAlpha, 1f))
            {
                isFadingIn = false;
            }
            return;
        }

        if (enableFadeMode && isFading)
        {
            if (Mathf.Approximately(newAlpha, 0f))
            {
                isFading = false;
                isWaiting = true;
                waitTimer = Random.Range(minWaitTime, maxWaitTime);
            }
        }
        else
        {
            // Update fluctuation logic
            fluctuationTimer += Time.fixedDeltaTime;
            targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.Sin(fluctuationTimer * fluctuationSpeed) * 0.5f + 0.5f);

            moveTimer += Time.fixedDeltaTime;
            if (enableFadeMode && moveTimer >= fluctuationDuration)
            {
                isFading = true;
                fadeTimer = 0f;
                targetAlpha = 0f; // Start fading out
                fluctuationDuration = Random.Range(minFluctuationDuration, maxFluctuationDuration); // Randomize fluctuation duration
            }
        }
    }

    private void ResetCloud()
    {
        transform.position = startPosition; // Use the defined start position
        spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f); // Start fully transparent
        isFading = false;
        isWaiting = false;
        isFadingIn = true;
        moveTimer = 0f; // Reset move timer
        targetAlpha = 1f; // Start fading in
        rotateClockwise = Random.value > 0.5f;
        fluctuationDuration = Random.Range(minFluctuationDuration, maxFluctuationDuration); // Randomize fluctuation duration
    }

    private CloudState RecordState()
    {
        return new CloudState
        {
            position = transform.position,
            color = spriteRenderer.color,
            isFading = isFading,
            fadeTimer = fadeTimer,
            isWaiting = isWaiting,
            waitTimer = waitTimer,
            rotation = currentRotation // Record rotation
        };
    }

    private void RestoreState(CloudState state)
    {
        transform.position = state.position;
        spriteRenderer.color = state.color;
        isFading = state.isFading;
        fadeTimer = state.fadeTimer;
        isWaiting = state.isWaiting;
        waitTimer = state.waitTimer;
        currentRotation = state.rotation; // Restore rotation
        transform.rotation = Quaternion.Euler(0f, 0f, currentRotation);
    }

    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindEnd.AddListener(ClearFutureStates);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindEnd.RemoveListener(ClearFutureStates);
    }

    private void ClearFutureStates()
    {
        int stateIndex = RewindTimeManager.instance.currentFrameIndex;

        // Delete future timeline
        if (stateIndex < cloudStates.Count - 1)
            cloudStates.RemoveRange(stateIndex + 1, cloudStates.Count - stateIndex - 1);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // Draw the starting position
        Gizmos.DrawSphere(transform.position, 0.1f);

        // Draw the movement path
        Vector3 endPosition = transform.position + (Vector3)(direction.normalized * speed * moveDuration);
        Gizmos.DrawLine(transform.position, endPosition);
        Gizmos.DrawSphere(endPosition, 0.1f);
    }
}

[System.Serializable]
public class CloudState
{
    public Vector3 position;
    public Color color;
    public bool isFading;
    public float fadeTimer;
    public bool isWaiting;
    public float waitTimer;
    public float rotation; // Store rotation
}
