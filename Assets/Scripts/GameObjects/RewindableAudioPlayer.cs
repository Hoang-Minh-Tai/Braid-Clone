using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class AudioState
{
    public int frameIndex;
    public bool start;
    public string groupName;
    public int clipIndex;     // -1 = no sound
}

[Serializable]
public class AudioClipGroup
{
    public string name;
    public AudioClip[] clips;
}


public class RewindableAudioPlayer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioSource audioSource;

    [Header("Sounds")]
    [SerializeField] private AudioClipGroup[] clips;

    // Cache for reversed clips: key = (groupName, clipIndex)
    private Dictionary<(string groupName, int clipIndex), AudioClip> reversedClipCache = new();

    // ----------------------------
    // REWIND DATA
    // ----------------------------
    private List<AudioState> audioStates = new();
    private int stateIndex;
    private bool isPlaying = false;

    private int currentClipIndex = -1;
    private string currentGroupName = "";
    public bool isUsingReversedClip = false;

    private RewindTimeManager rewindManager;

    private void Awake()
    {
        if (!audioSource)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.Play();
        rewindManager = RewindTimeManager.instance;

#if UNITY_WEBGL
        StartCoroutine(PreGenerateReversedClips());   // ← choose this or on-demand below
#endif
    }

#if UNITY_WEBGL
    private IEnumerator PreGenerateReversedClips()
    {
        yield return new WaitForSecondsRealtime(1f); // wait a moment for clips to load
        Debug.Log("******************** Create reversed clips for all AudioClipGroups... ********************");
        Debug.Log($"Total groups: {clips.Length}");

        foreach (var group in clips)
        {
            Debug.Log($"Processing group: {group.name} with {group.clips.Length} clips");
            for (int i = 0; i < group.clips.Length; i++)
            {
                var original = group.clips[i];
                if (original == null) continue;

                // Wait until the clip is fully loaded
                while (original.loadState != AudioDataLoadState.Loaded)
                {
                    yield return null;
                }

                var key = (group.name, i);
                if (!reversedClipCache.ContainsKey(key))
                {
                    var reversed = CreateReversedClip(original);
                    if (reversed != null)
                    {
                        reversedClipCache[key] = reversed;
                        Debug.Log($"Pre-generated reversed clip for {group.name} [{i}]");
                    }

                    else
                        Debug.LogWarning($"Failed to create reverse for {group.name} [{i}]");
                }
            }
        }
    }

    private AudioClip CreateReversedClip(AudioClip original)
    {
        if (original == null || original.samples == 0) return null;

        float[] data = new float[original.samples * original.channels];
        if (!original.GetData(data, 0))
        {
            Debug.LogError($"GetData failed for {original.name} - check Load Type = Decompress On Load");
            return null;
        }

        float[] reversedData = new float[data.Length];
        for (int i = 0; i < data.Length; i++)
        {
            reversedData[i] = data[data.Length - 1 - i];
        }

        AudioClip revClip = AudioClip.Create(
            original.name + "_Reversed",
            original.samples,
            original.channels,
            original.frequency,
            false   // !stream
        );

        revClip.SetData(reversedData, 0);
        return revClip;
    }

    private AudioClip GetReversedClip(string groupName, int clipIndex)
    {
        var key = (groupName, clipIndex);
        if (reversedClipCache.TryGetValue(key, out var rev) && rev != null)
            return rev;

        // On-demand fallback (if not pre-generated)
        var group = Array.Find(clips, g => g.name == groupName);
        if (group == null || clipIndex < 0 || clipIndex >= group.clips.Length) return null;

        var original = group.clips[clipIndex];
        var reversed = CreateReversedClip(original);
        if (reversed != null)
            reversedClipCache[key] = reversed;

        return reversed;
    }
#endif

    private void FixedUpdate()
    {
        if (RewindTimeManager.instance.rewinding)
        {
            if (audioStates.Count == 0) return;

            if (rewindManager.ReachStart || rewindManager.ReachEnd)
            {
                audioSource.pitch = 0;
                return;
            }
#if UNITY_WEBGL
            audioSource.pitch = Mathf.Abs(rewindManager.RewindSpeed);
#else
                        audioSource.pitch = rewindManager.RewindSpeed;
#endif
            HandleRewindingAudio();
            RestoreState();

            if (isPlaying && !audioSource.isPlaying)
            {
                isPlaying = false;
                audioSource.clip = null;
            }

            return;
        }

        HandleRewindingAudio();
        if (isPlaying && !audioSource.isPlaying)
        {
            isPlaying = false;
            AudioState state = RecordState(false);
            audioSource.clip = null;
            audioStates.Add(state);
            stateIndex = audioStates.Count - 1;
        }
    }

    private void HandleRewindingAudio()
    {
        if (!audioSource.isPlaying) return;
        bool forward = rewindManager.RewindSpeed > 0;
        // if forward while using reversed clip, switch to normal version but keep ratio. Do the same for backward.
        if (forward && isUsingReversedClip)
        {
            float currentRatio = 1 - audioSource.time / audioSource.clip.length;
            var group = Array.Find(clips, g => g.name == currentGroupName);
            if (group != null && currentClipIndex >= 0 && currentClipIndex < group.clips.Length)
            {
                audioSource.clip = group.clips[currentClipIndex];
                audioSource.time = audioSource.clip.length * currentRatio;
                audioSource.Play();
                isUsingReversedClip = false;
            }
        }
        else if (!forward && !isUsingReversedClip)
        {
            var revClip = GetReversedClip(currentGroupName, currentClipIndex);
            if (revClip != null)
            {
                float currentRatio = 1 - audioSource.time / audioSource.clip.length;
                audioSource.clip = revClip;
                audioSource.time = revClip.length * currentRatio;
                audioSource.pitch = Mathf.Abs(rewindManager.RewindSpeed);
                audioSource.Play();
                isUsingReversedClip = true;
            }
        }
    }

    // ----------------------------
    // PUBLIC API
    // ----------------------------
    public void Play(string groupName)
    {
        // Find the group with the specified name
        var group = Array.Find(clips, g => g.name == groupName);
        if (group == null || group.clips.Length == 0)
        {
            Debug.LogWarning($"RewindableAudioPlayer: AudioClipGroup '{groupName}' not found or has no clips.");
            return;
        }

        // Choose a random clip from the group
        int randomIndex = UnityEngine.Random.Range(0, group.clips.Length);
        currentClipIndex = randomIndex;
        currentGroupName = groupName;

        audioSource.clip = group.clips[randomIndex];
        audioSource.time = 0f;
        audioSource.pitch = 1f;
        audioSource.Play();

        isPlaying = true;
        AudioState state = RecordState(true);
        audioStates.Add(state);
        stateIndex = audioStates.Count - 1;
    }

    // public void Play(string groupName, bool saveState)
    // {
    //     // Find the group with the specified name
    //     var group = Array.Find(clips, g => g.name == groupName);
    //     if (group == null || group.clips.Length == 0)
    //         return;

    //     // Choose a random clip from the group
    //     int randomIndex = UnityEngine.Random.Range(0, group.clips.Length);
    //     currentClipIndex = randomIndex;
    //     currentGroupName = groupName;

    //     audioSource.clip = group.clips[randomIndex];
    //     audioSource.time = 0f;
    //     audioSource.pitch = 1f;
    //     audioSource.Play();

    //     if (!saveState) return;
    //     AudioState state = RecordState(true);
    //     audioStates.Add(state);
    //     stateIndex = audioStates.Count - 1;
    // }

    public void Play(string groupName, int clipIndex, bool saveState = true)
    {
        // Find the group with the specified name
        var group = System.Array.Find(clips, g => g.name == groupName);
        if (group == null || group.clips.Length == 0)
            return;

        // Validate clipIndex
        if (clipIndex < 0 || clipIndex >= group.clips.Length)
            return;

        currentClipIndex = clipIndex;
        currentGroupName = groupName;

        audioSource.clip = group.clips[clipIndex];
        audioSource.time = 0f;
        audioSource.pitch = 1f;
        audioSource.Play();

        if (!saveState) return;
        AudioState state = RecordState(true);
        audioStates.Add(state);
        stateIndex = audioStates.Count - 1;
    }

    public void Stop()
    {
        audioSource.Stop();
        isPlaying = false;

        AudioState state = RecordState(false);
        audioStates.Add(state);
        stateIndex = audioStates.Count - 1;
    }

    private void PlayBackward(string groupName, int clipIndex)
    {
        // Find the group with the specified name
        var group = System.Array.Find(clips, g => g.name == groupName);
        if (group == null || group.clips.Length == 0)
            return;

        // Validate clipIndex
        if (clipIndex < 0 || clipIndex >= group.clips.Length)
            return;

        currentClipIndex = clipIndex;
        currentGroupName = groupName;

#if UNITY_WEBGL
        AudioClip clipToUse = GetReversedClip(groupName, clipIndex);
        Debug.Log($"Playing backward clip: {clipToUse?.name}");
        audioSource.clip = clipToUse;
        audioSource.time = 0f;
#else
                audioSource.clip = group.clips[clipIndex];
                audioSource.time = audioSource.clip.length - 0.01f; // small offset avoids end-click
                audioSource.pitch = rewindManager.RewindSpeed; // negative = backward
#endif
        audioSource.Play();
        isPlaying = true;
    }

    // ----------------------------
    // STATE SYSTEM
    // ----------------------------
    private AudioState RecordState(bool start)
    {
        if (!start)
        {
            start = false;
        }

        return new AudioState
        {
            frameIndex = RewindTimeManager.instance.currentFrameIndex,
            clipIndex = currentClipIndex,
            groupName = currentGroupName,
            start = start
        };
    }

    private void RestoreState()
    {
        int currentFrameIndex = rewindManager.currentFrameIndex;
        bool backward = rewindManager.RewindSpeed < 0;

        AudioState stateToFind = null;
        for (int i = 0; i < audioStates.Count; i++)
        {
            var state = audioStates[i];
            if (backward)
            {
                if (state.frameIndex >= currentFrameIndex && state.frameIndex < currentFrameIndex + Math.Abs(rewindManager.RewindSpeed))
                    stateToFind = state;
            }
            else
            {
                if (state.frameIndex <= currentFrameIndex && state.frameIndex > currentFrameIndex - Math.Abs(rewindManager.RewindSpeed))
                    stateToFind = state;
            }
        }
        if (stateToFind == null) return;

        if (backward && stateToFind.start == false)
        {
            PlayBackward(stateToFind.groupName, stateToFind.clipIndex);
        }
        else if (!backward && stateToFind.start == true)
        {
            Play(stateToFind.groupName, stateToFind.clipIndex, false);
        }
    }

    // ----------------------------
    // REWIND CONTROL
    // ----------------------------
    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(OnRewindStart);
        events.onRewindEnd.AddListener(OnRewindEnd);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(OnRewindStart);
        events.onRewindEnd.RemoveListener(OnRewindEnd);
    }

    private void OnRewindStart(int rewindSpeed)
    {
    }

    private void OnRewindEnd()
    {
        audioSource.pitch = 1f;

        for (int i = audioStates.Count - 1; i >= 0; i--)
        {
            if (audioStates[i].frameIndex > rewindManager.currentFrameIndex)
            {
                audioStates.RemoveAt(i);
            }
        }
    }

    private void OnDestroy()
    {
#if UNITY_WEBGL
        foreach (var kvp in reversedClipCache)
        {
            if (kvp.Value != null) Destroy(kvp.Value);
        }
        reversedClipCache.Clear();
#endif
    }

    // ------------------------------ Debug ------------------------------
    [ContextMenu("Check audio playing")]
    private void CheckAudioPlaying()
    {
        Debug.Log($"Audio is playing: {audioSource.isPlaying}, clip: {audioSource.clip?.name}, time: {audioSource.time}");
    }
}
