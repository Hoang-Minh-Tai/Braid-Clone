using UnityEngine;

public class UI_DoorGuide : MonoBehaviour
{
    private SpriteRenderer image;
    private Coroutine blinkCoroutine;
    private Player player;

    public float blinkSpeed = 1f;

    private void Awake()
    {
        image = GetComponent<SpriteRenderer>();
        player = GetComponentInParent<Player>();
    }

    private void Start()
    {
        Hide();
    }

    private void OnEnable()
    {
        player.showDoorUIEvent.AddListener(ToggleUI);
    }

    private void ToggleUI(bool show)
    {
        if (show)
        {
            Show();
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        if (blinkCoroutine != null)
        {
            return;
        }
        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    private void Hide()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
        SetAlpha(0); // Ensure the UI is fully hidden
    }

    private System.Collections.IEnumerator BlinkEffect()
    {
        float duration = blinkSpeed; // Total time for one blink cycle
        while (true)
        {
            for (float t = 0; t < duration; t += Time.deltaTime)
            {
                // Use Mathf.Sin for a smooth transition and remap it to [0, 1]
                float alpha = (Mathf.Sin((t / duration) * Mathf.PI * 2) + 1) / 2;
                SetAlpha(alpha);
                yield return null;
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }
}
