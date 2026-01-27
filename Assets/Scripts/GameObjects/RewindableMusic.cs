using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class RewindableMusic : MonoBehaviour
{
    public static RewindableMusic instance;
    public AudioMixer audioMixer;

    [Header("Audio")]
    [SerializeField] private AudioSource source;
    [SerializeField] private AudioClip musicClip;

    [SerializeField] private AudioClip reversedClip;

    private RewindTimeManager rewindManager => RewindTimeManager.instance;

    private bool menuOpen;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        source.clip = musicClip;
        source.loop = true;
        source.pitch = 1f;
        source.Play();

    }

    void Start()
    {

        ApplySavedVolume();
    }

    public static void PlayMusic(AudioClip clip, AudioClip reverseClip)
    {
        if (instance == null) return;
        if (instance.source.clip == clip) return;

        instance.source.clip = clip;
        instance.musicClip = clip;
        instance.reversedClip = reverseClip;
        instance.source.Play();
    }

    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;

        events.onRewindStart.AddListener(EnableRewindMode);
        events.onRewindEnd.AddListener(DisableRewindMode);

        events.onMainMenuOpen.AddListener(PauseForMenu);
        events.onMainMenuClose.AddListener(ResumeFromMenu);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        var events = GameEventManager.instance.generalEvent;

        events.onRewindStart.RemoveListener(EnableRewindMode);
        events.onRewindEnd.RemoveListener(DisableRewindMode);

        events.onMainMenuOpen.RemoveListener(PauseForMenu);
        events.onMainMenuClose.RemoveListener(ResumeFromMenu);
    }

    private void FixedUpdate()
    {
        // Menu pause has highest priority
        if (menuOpen)
            return;

        if (rewindManager.rewinding)
        {
            ApplyPitch();

            // // Stop music at timeline bounds
            // if (rewindManager.currentFrameIndex == 0 ||
            //     rewindManager.currentFrameIndex == rewindManager.totalFrames)
            // {
            //     source.Pause();
            // }
            // else if (!source.isPlaying)
            // {
            //     source.UnPause();
            // }
        }
        else
        {
            // Normal playback
            if (!source.isPlaying)
            {
                source.Play();
            }

            source.pitch = 1f;
        }
    }

    private void EnableRewindMode(int rewindSpeed)
    {
        ApplyPitch();
    }

    private void DisableRewindMode()
    {
        source.pitch = 1f;
        // Change back to normal clip and keep the same time ratio
        if (source.clip != musicClip)
        {
            float currentRatio = (float)source.time / (float)source.clip.length;
            source.clip = musicClip;
            source.time = musicClip.length * (1f - currentRatio);
            source.Play();
        }
    }

    private void ApplyPitch()
    {
        int step = rewindManager.RewindSpeed;
        source.pitch = Mathf.Abs(step);
        if (step == 0) return;

        // If is playing forward but step is negative, switch to reversed clip but the pointer ratio should be 1 - currentRatio
        if (step < 0 && source.clip != reversedClip)
        {
            float currentRatio = (float)source.time / (float)source.clip.length;
            source.clip = reversedClip;
            source.time = reversedClip.length * (1f - currentRatio);
            source.Play();
        }
        // If is playing backward but step is positive, switch to normal clip but the pointer ratio should be 1 - currentRatio
        else if (step > 0 && source.clip != musicClip)
        {
            float currentRatio = (float)source.time / (float)source.clip.length;
            source.clip = musicClip;
            source.time = musicClip.length * (1f - currentRatio);
            source.Play();
        }
    }
    // private void ApplyPitch()
    // {
    //     int step = rewindManager.RewindSpeed;
    //     source.pitch = step;
    // }

    private void PauseForMenu()
    {
        menuOpen = true;
        source.Pause();
    }

    private void ResumeFromMenu()
    {
        menuOpen = false;
        source.UnPause();
    }

    private void ApplySavedVolume()
    {
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);

        // Prevent log(0)
        musicVolume = Mathf.Max(musicVolume, 0.0001f);

        audioMixer.SetFloat(
            "musicVolume",   // MUST match mixer exposed parameter
            Mathf.Log10(musicVolume) * 25f
        );

        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        // Prevent log(0)
        sfxVolume = Mathf.Max(sfxVolume, 0.0001f);

        audioMixer.SetFloat(
            "sfxVolume",   // MUST match mixer exposed parameter
            Mathf.Log10(sfxVolume) * 25f
        );
    }

}
