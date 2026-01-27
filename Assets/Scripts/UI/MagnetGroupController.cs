using Unity.Cinemachine;
using UnityEngine;

public class MagnetGroupController : MonoBehaviour
{
    public CinemachineTargetGroup TargetGroup;

    private CinemachineTargetGroup.Target player;

    void Awake()
    {
        player = TargetGroup.Targets[0];
    }

    void Update()
    {
        if (TargetGroup == null || TargetGroup.IsEmpty)
            return;

        // We assume that the player is at group index 0
        var targets = TargetGroup.Targets;
        var playerPos = targets[0].Object.position;

        bool foundMagnet = false;

        for (int i = 1; i < targets.Count; ++i)
        {
            var t = targets[i];
            if (t.Object != null && t.Object.TryGetComponent<Magnet>(out var magnet))
            {
                var distance = (playerPos - t.Object.position).magnitude;
                if (distance > magnet.Range)
                {
                    t.Weight = 0;
                    continue;
                }
                foundMagnet = true;

                float outerDistance = magnet.Range - distance;
                float blendDistance = magnet.Range - magnet.InnerRange;
                float ratio = outerDistance / blendDistance;

                t.Weight = magnet.Strength * Mathf.Min(1, ratio);
                player.Weight = 1 - t.Weight;
            }
        }

        if (!foundMagnet)
        {
            player.Weight = 1;
        }

    }
}