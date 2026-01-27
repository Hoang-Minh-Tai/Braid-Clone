using UnityEngine;

[ExecuteInEditMode]
public class Magnet : MonoBehaviour
{
    [Range(0.1f, 50.0f)]
    public float Strength = 5.0f;

    [Range(0.1f, 50.0f)]
    public float Range = 5.0f;
    [Range(0.0f, 50.0f)]
    public float InnerRange = 1.0f;

    public Transform RangeVisualizer;
    public Transform InnerRangeVisualizer;

    void Update()
    {
        if (RangeVisualizer != null)
            RangeVisualizer.localScale = new Vector3(Range * 2.0f, Range * 2.0f, 1);
        if (InnerRangeVisualizer != null)
            InnerRangeVisualizer.localScale = new Vector3(InnerRange * 2.0f, InnerRange * 2.0f, 1);
    }
}
