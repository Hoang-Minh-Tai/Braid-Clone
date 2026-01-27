using System;
using System.Collections.Generic;
using UnityEngine;

public class Player_EchoEffect : MonoBehaviour
{
    [Header("Echo")]
    [SerializeField] private GameObject playerEchoPrefab;

    [Header("Queue Size")]
    [SerializeField] private int queueSize = 0;
    [SerializeField] private int forwardBuffer = 2;

    [Header("Queue Smoothing")]
    [SerializeField] private float minChangeSpeed = 1f;   // linger near zero
    [SerializeField] private float maxChangeSpeed = 20f;  // snap at high speed
    [SerializeField] private float distanceForMaxSpeed = 10f;

    private readonly Queue<GameObject> activeEchos = new();

    private RewindTimeManager rewindManager => RewindTimeManager.instance;
    private Player_TimeShiftTrack timeShiftTrack;
    private List<PlayerTimeRecord> timeRecords;

    private float smoothedQueueSize;
    private int prevRecordStart = -1;
    private int prevRecordEnd = -1;

    private void Awake()
    {
        timeShiftTrack = GetComponent<Player_TimeShiftTrack>();
        timeRecords = timeShiftTrack.GetTimeTrack();
        smoothedQueueSize = queueSize;
    }

    private void FixedUpdate()
    {
        // -------------------------------
        // Target queue size
        // -------------------------------
        int targetQueueSize = Mathf.Abs(rewindManager.RewindSpeed) * 3;
        if (!rewindManager.rewinding)
            targetQueueSize = 0;

        // -------------------------------
        // Adaptive smoothing
        // -------------------------------
        float delta = Mathf.Abs(targetQueueSize - smoothedQueueSize);
        float t = Mathf.Clamp01(delta / distanceForMaxSpeed);
        float adaptiveSpeed = Mathf.Lerp(minChangeSpeed, maxChangeSpeed, t * t);

        smoothedQueueSize = Mathf.MoveTowards(
            smoothedQueueSize,
            targetQueueSize,
            adaptiveSpeed * Time.fixedDeltaTime
        );

        queueSize = Mathf.RoundToInt(smoothedQueueSize);

        // -------------------------------
        // Trim excess echoes (FIFO)
        // -------------------------------
        while (activeEchos.Count > queueSize)
        {
            var echo = activeEchos.Dequeue();
            if (echo != null)
                Destroy(echo);
        }

        if (queueSize == 0)
            return;

        // -------------------------------
        // Get records to show
        // -------------------------------
        int currentFrameIndex = rewindManager.currentFrameIndex;
        int recordStart;
        int recordEnd;
        bool backward = rewindManager.RewindSpeed < 0;

        if (backward)
        {
            recordStart = Mathf.Max(0, currentFrameIndex - forwardBuffer);
            recordEnd = Mathf.Min(timeRecords.Count - 1, recordStart + queueSize);
        }
        else
        {
            recordEnd = Mathf.Min(timeRecords.Count - 1, currentFrameIndex + forwardBuffer);
            recordStart = Mathf.Max(0, recordEnd - queueSize);
        }
        if (recordEnd == prevRecordEnd && recordStart == prevRecordStart)
            return; // no change
        // -------------------------------
        // Spawn echoes (no checks, no reuse)
        // -------------------------------
        for (int i = recordStart; i <= recordEnd; i++)
        {
            SpawnEcho(timeRecords[i]);
        }

        prevRecordStart = recordStart;
        prevRecordEnd = recordEnd;
    }

    private void SpawnEcho(PlayerTimeRecord record)
    {
        GameObject echo = Instantiate(
            playerEchoPrefab,
            record.position,
            Quaternion.identity
        );

        var sr = echo.GetComponent<SpriteRenderer>();
        sr.sprite = record.sprite;

        // Direction flip
        Vector3 scale = echo.transform.localScale;
        scale.x = record.direction * Math.Abs(scale.x);
        echo.transform.localScale = scale;

        activeEchos.Enqueue(echo);
    }
}
