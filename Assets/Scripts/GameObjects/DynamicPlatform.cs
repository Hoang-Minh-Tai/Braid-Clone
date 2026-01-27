using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
[RequireComponent(typeof(PlatformEffector2D))]
#endif
public class DynamicPlatformVisual : MonoBehaviour
{
#if UNITY_EDITOR
    public SpriteRenderer middle;
    public Transform start;
    public Transform end;

    [Header("Visual Settings")]
    public float endOffset;
    public int linkCount = 1;
    public float linkWidth = 0.4f; // sprite width in world units
    public bool flipX = false; // Added flipX variable

    [Header("Collider Settings")]
    public BoxCollider2D boxCollider;
    public float colliderXOffset = 0f;      // fine-tune alignment on X axis
    public float colliderWidthOffset = 0f;  // fine-tune width adjustment

    [Header("Sprite Renderer Visibility")]
    public bool showSprite = true;

    void Update()
    {
        // Only run in the Editor and not during Play Mode
        if (!Application.isEditor || Application.isPlaying) return;
        if (!middle || !start || !end) return;

        // Toggle SpriteRenderer visibility
        middle.enabled = showSprite;
        if (start.TryGetComponent<SpriteRenderer>(out var startRenderer))
        {
            startRenderer.enabled = showSprite;
        }
        if (end.TryGetComponent<SpriteRenderer>(out var endRenderer))
        {
            endRenderer.enabled = showSprite;
        }

        float totalWidth = linkCount * linkWidth;
        middle.size = new Vector2(totalWidth, middle.size.y);

        middle.transform.localPosition = new Vector3(totalWidth / 2f, 0, 0);
        end.localPosition = new Vector3(totalWidth + endOffset, end.localPosition.y, 0);

        // Flip the platform horizontally if flipX is true
        Vector3 scale = transform.localScale;
        scale.x = flipX ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;

        // Update collider
        if (boxCollider)
        {
            float colliderWidth = totalWidth + endOffset * 2f + colliderWidthOffset;
            boxCollider.size = new Vector2(colliderWidth, boxCollider.size.y);
            boxCollider.offset = new Vector2(totalWidth / 2f + colliderXOffset, boxCollider.offset.y);
        }
    }
#endif
}
