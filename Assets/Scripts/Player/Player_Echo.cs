using UnityEngine;
using System.Collections;

public class Player_Echo : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    [Header("Fade In")]
    [SerializeField] private float fadeInDuration = 0.15f;
    [SerializeField, Range(0f, 1f)] private float targetAlpha = 0.5f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (spriteRenderer == null)
            return;

        // Reset alpha before fading in
        Color c = spriteRenderer.color;
        c.a = 0f;
        spriteRenderer.color = c;

        StartCoroutine(FadeIn());
    }

    public void SetSprite(Sprite newSprite)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = newSprite;
    }

    private IEnumerator FadeIn()
    {
        float elapsed = 0f;
        Color c = spriteRenderer.color;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            c.a = Mathf.Lerp(0f, targetAlpha, elapsed / fadeInDuration);
            spriteRenderer.color = c;
            yield return null;
        }

        c.a = targetAlpha;
        spriteRenderer.color = c;
    }
}
