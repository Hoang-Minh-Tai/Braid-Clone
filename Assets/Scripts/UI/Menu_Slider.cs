using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;
using UnityEngine.UI;

public class Menu_Slider : Menu_Option
{
    [Header("Slider Settings")]
    [SerializeField]
    private Slider slider;
    [SerializeField]
    private float valueChangeRate = 0.3f; // Rate at which the slider value changes per input

    [SerializeField]
    private AudioMixer audioMixer;
    [SerializeField]
    private float mixerMultiplier = 25f;
    [SerializeField]
    private string audioParameterName;

    public UnityEvent onSelect;

    [Header("Slider Graphics")]
    [SerializeField] private Image background;
    [SerializeField] private Image fill;
    [SerializeField] private Image handle;
    [SerializeField] private Image knob1;
    [SerializeField] private Image knob2;

    [Header("Colors")]
    [SerializeField] private Color handleNormal = Color.white;
    [SerializeField] private Color handleDisabled = Color.gray;

    [SerializeField] private Color fillNormal = Color.white;
    [SerializeField] private Color fillDisabled = Color.gray;

    [SerializeField] private Color backgroundNormal = new Color(0.5f, 0.5f, 0.5f, 1f);
    [SerializeField] private Color backgroundDisabled = Color.black;

    protected override void Awake()
    {
        base.Awake();
    }

    void OnEnable()
    {
    }

    void Start()
    {
    }

    void Update()
    {
        slider.value += valueChange * valueChangeRate * Time.unscaledDeltaTime;
        slider.value = Mathf.Clamp01(slider.value);
    }

    public void SliderValueChanged()
    {
        audioMixer.SetFloat(audioParameterName, Mathf.Log10(slider.value) * mixerMultiplier);
    }

    public override void Select()
    {
        // Implement slider-specific selection logic if needed
    }

    public void SetValue(float value)
    {
        if (slider != null)
        {
            slider.value = value;
        }
    }

    public float GetValue()
    {
        return slider != null ? slider.value : 0f;
    }

    public override void Hover(bool hover)
    {
        base.Hover(hover);
        if (text == null)
            return;

        text.color = hover ? Color.white : Color.black;
        ApplyVisual(hover);
    }

    private void ApplyVisual(bool hover)
    {
        knob1.color = hover ? handleNormal : handleDisabled;
        knob2.color = hover ? handleNormal : handleDisabled;
        handle.color = hover ? handleNormal : handleDisabled;
        fill.color = hover ? fillNormal : fillDisabled;
        background.color = hover ? backgroundNormal : backgroundDisabled;
    }
}
