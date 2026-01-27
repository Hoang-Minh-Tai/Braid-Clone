using UnityEngine;
using System.Collections;

public class UI_AudioSetting : UI_Page
{
    [Header("Sample Audio Clips")]
    [SerializeField] private AudioClip[] sampleSFXClips;
    [SerializeField] private AudioClip sampleMusicClip;
    [SerializeField] private AudioSource audioSourceMusic;
    [SerializeField] private AudioSource audioSourceSFX;

    [Header("Audio Settings")]
    [SerializeField] private Menu_Slider musicSlider;
    [SerializeField] private Menu_Slider sfxSlider;

    protected override void OnEnable()
    {
        base.OnEnable();
        Debug.Log($"UI Audio Setting Page '{gameObject.name}' Enabled!");
        float musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        float sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);

        musicSlider.SetValue(musicVolume);
        sfxSlider.SetValue(sfxVolume);

        // Debug.Log("Loaded Music Volume: " + musicVolume);
        // Debug.Log("Loaded SFX Volume: " + sfxVolume);

        // Debug.Log("Music Slider Value: " + musicSlider.GetValue());
        // Debug.Log("SFX Slider Value: " + sfxSlider.GetValue());
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.GetValue());
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.GetValue());

        Debug.Log("Saved Music Volume: " + musicSlider.GetValue());
        Debug.Log("Saved SFX Volume: " + sfxSlider.GetValue());
    }

    public override void PageEnter()
    {
        base.PageEnter();

        // Play the sample music clip in a loop
        audioSourceMusic.clip = sampleMusicClip;
        audioSourceMusic.loop = true;
        audioSourceMusic.Play();

        // Start playing random sample SFX clips continuously
        StartCoroutine(PlayRandomSFX());
    }

    private IEnumerator PlayRandomSFX()
    {
        while (true)
        {
            int randomIndex = Random.Range(0, sampleSFXClips.Length);
            audioSourceSFX.PlayOneShot(sampleSFXClips[randomIndex]);

            yield return new WaitUntil(() => !audioSourceSFX.isPlaying);
            // Wait for a random interval before playing the next SFX
            yield return new WaitForSecondsRealtime(1f);
        }
    }

    public override void PageExit()
    {
        base.PageExit();

        // Stop the music and SFX playback
        audioSourceMusic.Stop();
        StopCoroutine(PlayRandomSFX());
    }

}