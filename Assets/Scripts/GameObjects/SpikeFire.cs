using UnityEngine;

[ExecuteAlways]
public class SpikeFire : MonoBehaviour
{
    private SpriteRenderer spikeRenderer;
    private MaterialPropertyBlock mpb;

    private RewindTimeManager rewindTimeManager =>
        Application.isPlaying ? RewindTimeManager.instance : null;

    [Header("Fire Settings")]
    private float speed = 0.04f;
    [SerializeField] private float tile = 1f;

    void Awake()
    {
        Init();
        ApplyProperties();
    }

    void OnEnable()
    {
        Init();
        ApplyProperties();
    }


    void FixedUpdate()
    {
        if (!Application.isPlaying) return;

        ApplyProperties();
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        Init();
        ApplyProperties();
    }
#endif

    private void Init()
    {
        if (spikeRenderer == null)
            spikeRenderer = GetComponent<SpriteRenderer>();

        if (mpb == null)
            mpb = new MaterialPropertyBlock();
    }

    private void ApplyProperties()
    {
        if (spikeRenderer == null) return;

        spikeRenderer.GetPropertyBlock(mpb);

        mpb.SetFloat("_TileX", tile);
        mpb.SetFloat("_GameSpeed", speed);

        if (rewindTimeManager != null)
            mpb.SetFloat("_FrameIndex", rewindTimeManager.currentFrameIndex);
        else
            mpb.SetFloat("_FrameIndex", 0); // editor preview frame

        spikeRenderer.SetPropertyBlock(mpb);
    }
}
