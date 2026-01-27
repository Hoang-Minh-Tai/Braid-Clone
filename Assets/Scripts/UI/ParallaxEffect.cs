using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Range(0f, 1f)]
    public float parallaxMultiplier = 0.01f;

    [SerializeField] private Vector3 scenePivot;

    private Transform cam;
    private Vector3 startPos;

    void Start()
    {
        cam = Camera.main.transform;
        startPos = transform.position;
    }

    void Update()
    {
        float camOffsetX = cam.position.x - scenePivot.x;
        transform.position = startPos + new Vector3(camOffsetX * parallaxMultiplier, 0f, 0f);
    }
}
