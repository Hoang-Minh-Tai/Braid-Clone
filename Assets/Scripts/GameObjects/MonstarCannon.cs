using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CannonState
{
    public Vector3 position;
    public int activeMonsterCount;
    public float timeSinceLastFire;
}

public class MonstarCannon : MonoBehaviour
{
    [Header("Cannon Settings")]
    [SerializeField] private Mob_HedgeHog[] monsterPool; // Pool of monsters
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireResetTime = 1f;
    [SerializeField] private float fireForce = 10f;
    [SerializeField, Range(0f, 360f)] private float fireAngle = 90f; // Fixed fire angle in degrees

    [Header("Editor Only")]
    [SerializeField] private bool flipX = false; // Flip the cannon in the editor

    private int activeMonsterCount = 0;
    private int monsterCount;
    private float timeSinceLastFire = 0f;
    private bool rewinding = false;
    private List<CannonState> cannonStates = new();
    private int stateIndex = 0;

    private bool shouldFireNow => !rewinding && timeSinceLastFire >= fireResetTime && activeMonsterCount < monsterCount;

    private readonly int[] rewindSteps = { -8, -4, -2, -1, 0, 1, 2, 4, 8 };
    private int speedIndex = 4;

    private void Start()
    {
        monsterCount = monsterPool.Length;
        InitializeMonsterPool();
    }

    private void OnEnable() => RegisterEvents();
    private void OnDisable() => UnregisterEvents();

    private void FixedUpdate()
    {
        if (rewinding)
        {
            HandleRewind();
            return;
        }

        // Only update timer if we still have monsters to fire
        if (activeMonsterCount < monsterCount)
            timeSinceLastFire += Time.fixedDeltaTime;

        // Fire when ready
        if (timeSinceLastFire >= fireResetTime && activeMonsterCount < monsterCount)
        {
            Fire();
            timeSinceLastFire = 0f;
        }

        // Record state AFTER updating the cannon
        cannonStates.Add(RecordState());
        stateIndex = cannonStates.Count - 1;
    }


    private void InitializeMonsterPool()
    {
        foreach (var monster in monsterPool)
        {
            monster.onDeadEvent.AddListener(OnMonsterDeath); // Use named method
                                                             // Ensure starting as dead/inactive if intended by your pool design:
            monster.Dead = true; // uncomment if you want pool monsters to start 'Dead'
        }
        activeMonsterCount = 0;
    }

    private void Fire()
    {
        if (firePoint == null) return;

        Mob_HedgeHog monster = GetInactiveMonster();
        if (monster == null) return;
        monster.Dead = false;

        monster.transform.position = firePoint.position;

        float angle = fireAngle;
        if (flipX)
            angle = 180f - angle;

        Vector2 fireDirection = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;

        monster.SetDirection(fireDirection.x);

        Rigidbody2D rb = monster.rb;

        rb.linearVelocity = fireDirection * fireForce;
        monster.isGrounded = false;

        activeMonsterCount++;
    }


    private Mob_HedgeHog GetInactiveMonster()
    {
        foreach (var monster in monsterPool)
        {
            if (monster.Dead)
            {
                return monster;
            }
        }
        return null; // No inactive monster found
    }

    private void HandleRewind()
    {
        if (cannonStates.Count == 0) return;

        int step = rewindSteps[speedIndex];
        stateIndex += step;
        stateIndex = Mathf.Clamp(stateIndex, 0, cannonStates.Count - 1);

        RestoreState(cannonStates[stateIndex]);
    }

    private void RegisterEvents()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(OnRewindStart);
        events.onRewindEnd.AddListener(OnRewindEnd);
        events.onRewindBackward.AddListener(OnRewindBackward);
        events.onRewindForward.AddListener(OnRewindForward);
    }

    private void UnregisterEvents()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(OnRewindStart);
        events.onRewindEnd.RemoveListener(OnRewindEnd);
        events.onRewindBackward.RemoveListener(OnRewindBackward);
        events.onRewindForward.RemoveListener(OnRewindForward);
    }

    private void OnRewindStart(int speed)
    {
        rewinding = true;
        speedIndex = speed == 0 ? 4 : 3;
        // leave stateIndex where it is (it should point to the latest recorded state)
        // if there are no states yet, clamp it
        stateIndex = Mathf.Clamp(stateIndex, 0, Mathf.Max(0, cannonStates.Count - 1));
    }

    // IMPORTANT: when rewind ends we should stop rewinding and delete future states
    private void OnRewindEnd()
    {
        rewinding = false;

        // Delete all states AFTER the current point (same behavior as Gate)
        if (stateIndex < cannonStates.Count - 1)
            cannonStates.RemoveRange(stateIndex + 1, cannonStates.Count - (stateIndex + 1));
    }

    private void OnRewindBackward()
    {
        speedIndex = Mathf.Clamp(speedIndex - 1, 0, rewindSteps.Length - 1);
    }
    private void OnRewindForward()
    {
        speedIndex = Mathf.Clamp(speedIndex + 1, 0, rewindSteps.Length - 1);
    }

    private void OnMonsterDeath()
    {
        activeMonsterCount = Mathf.Max(0, activeMonsterCount - 1);
    }

    private CannonState RecordState()
    {
        return new CannonState
        {
            position = transform.position,
            activeMonsterCount = activeMonsterCount,
            timeSinceLastFire = timeSinceLastFire
        };
    }

    private void RestoreState(CannonState state)
    {
        // restore transform and counters
        transform.position = state.position;
        activeMonsterCount = state.activeMonsterCount;
        timeSinceLastFire = state.timeSinceLastFire;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (firePoint != null)
        {
            float angle = fireAngle;
            if (flipX)
                angle = 180f - angle;

            Vector2 fireDirection = new Vector2(
                Mathf.Cos(angle * Mathf.Deg2Rad),
                Mathf.Sin(angle * Mathf.Deg2Rad)
            ).normalized;

            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + (Vector3)fireDirection * 2f);
        }
    }

    private void OnValidate()
    {
        // Flip the cannon in the editor based on the flipX value
        Vector3 localScale = transform.localScale;
        localScale.x = flipX ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
        transform.localScale = localScale;
    }
#endif
}
