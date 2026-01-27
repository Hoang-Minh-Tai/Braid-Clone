using UnityEngine;
using UnityEngine.Events;

public class AnimationTrigger : MonoBehaviour
{
    public UnityEvent snapEvent;

    public void TriggerSnap()
    {
        snapEvent.Invoke();
    }
}
