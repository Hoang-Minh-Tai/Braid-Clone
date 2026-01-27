using TMPro;
using UnityEngine;

public abstract class Menu_Option : MonoBehaviour
{
    protected TextMeshProUGUI text;
    protected bool onHover;
    protected float hoverTime;
    public float valueChange;
    public bool isDisabled = false;

    public abstract void Select();

    public virtual void Hover(bool hover)
    {
        onHover = hover;
    }

    protected virtual void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        text.color = Color.black;
    }
}
