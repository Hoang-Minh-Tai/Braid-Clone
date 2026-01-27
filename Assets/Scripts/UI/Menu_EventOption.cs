using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class Menu_EventOption : Menu_Option
{

    [Header("Hover Animation")]
    [SerializeField] private float cycleDuration = 1.5f;
    [SerializeField] private Color disableColor = new Color(150, 150, 150);

    public UnityEvent onSelect;

    protected override void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.color = isDisabled ? disableColor : Color.black;
    }

    void Update()
    {
        if (!onHover || isDisabled)
            return;

        hoverTime += Time.unscaledDeltaTime; // Use unscaledDeltaTime to ensure hover animation works when timeScale is 0

        float t = Mathf.PingPong(hoverTime / cycleDuration, 1f);
        text.color = EvaluateColor(t);
    }

    public void SetDisabled(bool disabled)
    {
        isDisabled = disabled;
        text.color = isDisabled ? disableColor : Color.black;
    }

    public override void Select()
    {
        if (isDisabled)
            return;
        onSelect?.Invoke();
    }

    public override void Hover(bool hover)
    {
        if (isDisabled)
            return;
        onHover = hover;
        if (text == null)
            return;

        if (!hover)
        {
            hoverTime = 0f;
            text.color = Color.black;
        }
    }

    private Color EvaluateColor(float t)
    {
        // 0 → Black
        // 0.25 → Red
        // 0.5 → White
        // 0.75 → Red
        // 1 → Black

        if (t < 0.25f)
            return Color.Lerp(Color.black, Color.red, t / 0.25f);
        else if (t < 0.5f)
            return Color.Lerp(Color.red, Color.white, (t - 0.25f) / 0.25f);
        else if (t < 0.75f)
            return Color.Lerp(Color.white, Color.red, (t - 0.5f) / 0.25f);
        else
            return Color.Lerp(Color.red, Color.black, (t - 0.75f) / 0.25f);
    }
}
