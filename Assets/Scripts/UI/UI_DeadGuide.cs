using UnityEngine;

public class UI_DeadGuide : MonoBehaviour
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
        Hide(0);
    }

    private void OnEnable()
    {
        GameEventManager.instance.generalEvent.onDeadHitBottom.AddListener(Show);
        GameEventManager.instance.generalEvent.onRewindStart.AddListener(Hide);
    }

    private void OnDisable()
    {
        GameEventManager.instance.generalEvent.onDeadHitBottom.RemoveListener(Show);
        GameEventManager.instance.generalEvent.onRewindStart.RemoveListener(Hide);
    }

    private void Show()
    {
        if (blinkCoroutine != null)
        {
            return;
        }
        blinkCoroutine = StartCoroutine(BlinkEffect());
    }

    private void Hide(int value)
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

        // Check the parent's scale x value and adjust this object's scale to avoid flipping
        if (player != null && player.transform.localScale.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

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
