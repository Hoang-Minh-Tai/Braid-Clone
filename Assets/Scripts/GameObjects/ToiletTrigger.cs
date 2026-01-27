using UnityEngine;
using System.Collections.Generic; // Add this line for using List

public class ToiletTrigger : MonoBehaviour
{
    [SerializeField] private RewindableAudioPlayer audioPlayer;
    [SerializeField] private GameObject lightEffect;

    private RewindTimeManager rewindManager;
    private List<LightToggleState> lightToggleStates = new(); // List to save frame indices and states

    void Awake()
    {
        rewindManager = RewindTimeManager.instance;
    }

     private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindEnd.AddListener(OnRewindEnd);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindEnd.RemoveListener(OnRewindEnd);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (rewindManager.rewinding) return;
        if (lightEffect.activeSelf) return;

        if (collision.CompareTag("Player"))
        {
            audioPlayer.Play("switch");
            lightEffect.SetActive(true);

            // Save the frame and state when the light is turned on
            lightToggleStates.Add(new LightToggleState
            {
                frameIndex = rewindManager.currentFrameIndex,
                isOn = true
            });
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (rewindManager.rewinding) return;
        if (!lightEffect.activeSelf) return;
        
        if (collision.CompareTag("Player"))
        {
            audioPlayer.Play("switch");
            lightEffect.SetActive(false);

            // Save the frame and state when the light is turned off
            lightToggleStates.Add(new LightToggleState
            {
                frameIndex = rewindManager.currentFrameIndex,
                isOn = false
            });
        }
    }

    private void FixedUpdate()
    {
        if (rewindManager.rewinding)
        {
            int currentFrameIndex = rewindManager.currentFrameIndex;
            int rewindSpeed = Mathf.Abs(rewindManager.RewindSpeed);
            bool backward = rewindManager.RewindSpeed < 0;

            LightToggleState stateToApply = null;
            foreach (var state in lightToggleStates)
            {
                if (backward)
                {
                    if (state.frameIndex >= currentFrameIndex && state.frameIndex < currentFrameIndex + rewindSpeed)
                    {
                        stateToApply = state;
                        break;
                    }
                }
                else
                {
                    if (state.frameIndex <= currentFrameIndex && state.frameIndex > currentFrameIndex - rewindSpeed)
                    {
                        stateToApply = state;
                        break;
                    }
                }
            }

            if (stateToApply != null)
            {
                lightEffect.SetActive(backward ? !stateToApply.isOn : stateToApply.isOn);
            }
        }
    }

    private void OnRewindEnd()
    {
        // Remove future states beyond the current frame index
        for (int i = lightToggleStates.Count - 1; i >= 0; i--)
        {
            if (lightToggleStates[i].frameIndex > rewindManager.currentFrameIndex)
            {
                lightToggleStates.RemoveAt(i);
            }
        }
    }

    [System.Serializable]
    private class LightToggleState
    {
        public int frameIndex;
        public bool isOn;
    }
}
