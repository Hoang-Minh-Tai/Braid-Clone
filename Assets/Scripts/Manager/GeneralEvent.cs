using UnityEngine;
using UnityEngine.Events;

public class GeneralEvent
{
    // Event triggered when rewind starts
    public UnityEvent<int> onRewindStart = new UnityEvent<int>();

    // Event triggered when rewinding backward
    public UnityEvent onRewindBackward = new UnityEvent();

    // Event triggered when rewinding forward
    public UnityEvent onRewindForward = new UnityEvent();

    // Event triggered when rewind ends
    public UnityEvent onRewindEnd = new UnityEvent();

    public UnityEvent onDeadHitBottom = new UnityEvent();
    public UnityEvent<PuzzleLocal> onPuzzleCollect = new UnityEvent<PuzzleLocal>();

    public UnityEvent onOpenDoor = new UnityEvent();
    public UnityEvent onMainMenuOpen = new UnityEvent();
    public UnityEvent onMainMenuClose = new UnityEvent();


    // Call this to start rewind
    public void StartRewind(int speed = -1)
    {
        onRewindStart.Invoke(speed);
    }

    // Call this while rewinding backward
    public void RewindBackward()
    {
        onRewindBackward.Invoke();
    }

    // Call this while rewinding forward
    public void RewindForward()
    {
        onRewindForward.Invoke();
    }

    // Call this to end rewind
    public void EndRewind()
    {
        onRewindEnd.Invoke();
    }

    public void DeadHitBottom()
    {
        onDeadHitBottom.Invoke();
    }

    public void PuzzleCollect(PuzzleLocal puzzle)
    {
        onPuzzleCollect.Invoke(puzzle);
    }

    public void OpenDoor()
    {
        onOpenDoor.Invoke();
    }

    public void OpenMainMenu()
    {
        onMainMenuOpen.Invoke();
    }
    public void CloseMainMenu()
    {
        onMainMenuClose.Invoke();
    }
}
