using System;
using System.Collections.Generic;
using UnityEngine;

public struct FrameEcho
{
    public int frameIndex;
    public GameObject echo;

    public FrameEcho(int frameIndex, GameObject echo)
    {
        this.frameIndex = frameIndex;
        this.echo = echo;
    }
}

/// <summary>
/// A frame-ordered container.
/// Items can be enqueued in any order,
/// but are always stored sorted by frameIndex.
/// </summary>
public class FrameQueue
{
    // LinkedList is ideal for ordered insertion + head/tail removal
    private readonly LinkedList<FrameEcho> records = new();

    /// <summary>
    /// Insert a record in correct frame order.
    /// </summary>
    public void Enqueue(FrameEcho record)
    {
        if (records.Count == 0)
        {
            records.AddFirst(record);
            return;
        }

        // Fast path: newest frame
        if (record.frameIndex > records.Last.Value.frameIndex)
        {
            records.AddLast(record);
            return;
        }

        // Fast path: oldest frame
        if (record.frameIndex < records.First.Value.frameIndex)
        {
            records.AddFirst(record);
            return;
        }

        // Insert somewhere in the middle
        var node = records.Last;
        while (node != null && node.Value.frameIndex > record.frameIndex)
        {
            node = node.Previous;
        }

        // Prevent duplicate frame indices (optional but recommended)
        if (node != null && node.Value.frameIndex == record.frameIndex)
        {
            // Replace existing record
            node.Value = record;
            return;
        }

        records.AddAfter(node, record);
    }

    /// <summary>
    /// Remove and return the oldest frame (smallest frameIndex).
    /// </summary>
    public FrameEcho DequeueOldest()
    {
        if (records.Count == 0)
            throw new InvalidOperationException("FrameQueue is empty");

        var record = records.First.Value;
        records.RemoveFirst();
        return record;
    }

    /// <summary>
    /// Remove and return the newest frame (largest frameIndex).
    /// </summary>
    public FrameEcho DequeueNewest()
    {
        if (records.Count == 0)
            throw new InvalidOperationException("FrameQueue is empty");

        var record = records.Last.Value;
        records.RemoveLast();
        return record;
    }

    /// <summary>
    /// Peek oldest without removing.
    /// </summary>
    public FrameEcho PeekOldest() => records.First.Value;

    /// <summary>
    /// Peek newest without removing.
    /// </summary>
    public FrameEcho PeekNewest() => records.Last.Value;

    public int Count => records.Count;

    public void Clear()
    {
        records.Clear();
    }
}
