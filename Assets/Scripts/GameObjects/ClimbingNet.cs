using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
#endif
public class ClimbingNet : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer netBody;

    // 4 limit points
    public Transform topPoint;
    public Transform bottomPoint;
    public Transform leftPoint;
    public Transform rightPoint;

    [Header("Colliders")]
    public BoxCollider2D climbCollider;       // main climbing area

    [Header("Detection Collider")]
    public BoxCollider2D detectionCollider;   // slightly bigger
    public float detectOffsetX = 0f;
    public float detectOffsetY = 0f;
    public float detectWidthOffset = 0f;
    public float detectHeightOffset = 0f;

    public bool inRange;

    private void Update()
    {
        if (!Application.isEditor || Application.isPlaying) return;
        if (!climbCollider || !netBody) return;

        // -------------------------------------------------------------------
        // 1. Get climb collider world/local bounds
        // -------------------------------------------------------------------
        Vector2 size = climbCollider.size;
        Vector2 offset = climbCollider.offset;

        float halfW = size.x * 0.5f;
        float halfH = size.y * 0.5f;

        // -------------------------------------------------------------------
        // 2. Position the net body (scale sprite to fit)
        // -------------------------------------------------------------------
        netBody.size = size;  // match collider area

        // -------------------------------------------------------------------
        // 3. Move limit points
        // -------------------------------------------------------------------
        if (topPoint)
            topPoint.localPosition = new Vector3(offset.x, offset.y + halfH, 0);

        if (bottomPoint)
            bottomPoint.localPosition = new Vector3(offset.x, offset.y - halfH, 0);

        if (leftPoint)
            leftPoint.localPosition = new Vector3(offset.x - halfW, offset.y, 0);

        if (rightPoint)
            rightPoint.localPosition = new Vector3(offset.x + halfW, offset.y, 0);

        // -------------------------------------------------------------------
        // 4. Update detection collider (slightly bigger area)
        // -------------------------------------------------------------------
        if (detectionCollider)
        {
            detectionCollider.size = new Vector2(
                size.x + detectWidthOffset,
                size.y + detectHeightOffset
            );

            detectionCollider.offset = new Vector2(
                offset.x + detectOffsetX,
                offset.y + detectOffsetY
            );
        }
    }
}
