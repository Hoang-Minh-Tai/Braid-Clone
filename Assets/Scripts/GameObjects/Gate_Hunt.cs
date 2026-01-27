using System;
using UnityEngine;


[System.Serializable]
public class GateHuntState : GateState
{
    public bool[] markEnabled;
}


public class Gate_Hunt : Gate
{
    [Header("Hunt Gate")]
    [SerializeField] private GameObject[] redMarks = new GameObject[6];
    [SerializeField]
    private bool[] markEnabled = new bool[6];


    protected override GateState RecordState()
    {
        GateHuntState state = new GateHuntState();

        // --- Copy base Gate state ---
        base.FillBaseState(state);

        // --- Copy hunt marks ---
        state.markEnabled = (bool[])markEnabled.Clone();

        return state;
    }

    protected override void RestoreState(GateState genericState)
    {
        base.RestoreState(genericState);

        GateHuntState state = (GateHuntState)genericState;

        Array.Copy(state.markEnabled, markEnabled, 6);

        // Apply to GameObjects
        for (int i = 0; i < 6; i++)
        {
            if (redMarks[i] != null)
                redMarks[i].SetActive(markEnabled[i]);
        }
    }

    /// <summary>
    /// Enable or disable a red mark by index (0 - 5).
    /// This will also be saved for rewind.
    /// </summary>
    public void SetMark(int index)
    {
        if (index < 0 || index >= 6)
        {
            Debug.LogWarning($"Gate_Hunt: Mark index {index} out of range!");
            return;
        }

        markEnabled[index] = true;

        if (redMarks[index] != null)
            redMarks[index].SetActive(true);

        // Check if all marks are set
        if (AllMarksSet())
        {
            Open();
        }
    }

    /// <summary>
    /// Checks if all marks are set.
    /// </summary>
    private bool AllMarksSet()
    {
        foreach (bool mark in markEnabled)
        {
            if (!mark) return false;
        }
        return true;
    }
}
