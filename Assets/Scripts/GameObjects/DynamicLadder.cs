using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class DynamicLadder : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer body;
    public Transform top;
    public Transform topPoint; // Added top point
    public Transform bottomPoint; // Added bottom point

    [Header("Size Settings")]
    public int rungCount = 5;
    public float rungHeight = 0.4f;
    public float topOffset = 0.1f;

    [Header("Collider Settings")]
    public BoxCollider2D ladderCollider;
    public float colliderYOffset = 0f;        // fine-tune vertical offset
    public float colliderHeightOffset = 0f;   // fine-tune height

    [Header("Ladder Detection Settings")]
    public BoxCollider2D ladderDetectionCollider;
    public float detectionYOffset = 0f;       // fine-tune vertical offset
    public float detectionHeightOffset = 0f;  // fine-tune height
    public float detectionXOffset = 0f;       // fine-tune horizontal alignment
    public float detectionWidthOffset = 0f;   // fine-tune width (optional)

    private void Update()
    {
        if (!Application.isEditor || Application.isPlaying) return;
        if (!body || !top) return;

        // Calculate total ladder height
        float totalHeight = rungCount * rungHeight;

        // Adjust ladder body
        body.size = new Vector2(body.size.x, totalHeight);
        body.transform.localPosition = new Vector3(0, totalHeight / 2f, 0);

        // Position top piece
        top.localPosition = new Vector3(0, totalHeight + topOffset, 0);

        // Adjust main climb collider
        if (ladderCollider)
        {
            float colliderHeight = totalHeight + topOffset + colliderHeightOffset;
            ladderCollider.size = new Vector2(ladderCollider.size.x, colliderHeight);
            ladderCollider.offset = new Vector2(0, totalHeight / 2f + colliderYOffset);
        }

        // Adjust detection collider
        if (ladderDetectionCollider)
        {
            float detectionHeight = totalHeight + topOffset + detectionHeightOffset;
            float detectionWidth = ladderDetectionCollider.size.x + detectionWidthOffset;

            ladderDetectionCollider.size = new Vector2(detectionWidth, detectionHeight);
            ladderDetectionCollider.offset = new Vector2(detectionXOffset, totalHeight / 2f + detectionYOffset);
        }

        // Adjust top and bottom points
        if (topPoint)
        {
            topPoint.localPosition = new Vector3(0, totalHeight + topOffset, 0);
        }

    }
}
