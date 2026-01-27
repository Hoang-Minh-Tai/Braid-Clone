using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UI_Page : MonoBehaviour
{
    private PlayerControl input;
    [SerializeField] private RectMask2D containerMask;
    [SerializeField] private float targetPadding = 100f;

    [SerializeField] private Menu_Option[] options;
    public UI_Page parentPage;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip pageTransitionClip;
    [SerializeField] private AudioClip pageExitClip;

    private AudioSource audioSource;

    private bool isTransitioning;

    private CanvasGroup canvasGroup;

    private int currentOptionIndex;

    private void Awake()
    {
        input = InputManager.Instance.Input;
        options = GetComponentsInChildren<Menu_Option>();
        currentOptionIndex = 0;
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (options != null && options.Length > 0)
            options[currentOptionIndex].Hover(true);
    }

    protected virtual void OnEnable()
    {
        input.UI.Cancel.performed += OnCancel;
        if (options != null && options.Length > 0)
        {
            input.UI.Navigate.performed += OnNavigate;
            input.UI.Select.performed += OnOptionSelect;

            UpdateSelection();
        }
    }

    protected virtual void OnDisable()
    {
        input.UI.Cancel.performed -= OnCancel;
        input.UI.Navigate.performed -= OnNavigate;
        input.UI.Select.performed -= OnOptionSelect;
    }

    void Update()
    {
        if (options == null || options.Length == 0) return;
        options[currentOptionIndex].valueChange = input.UI.ChangeValue.ReadValue<float>();
    }

    private void OnOptionSelect(InputAction.CallbackContext ctx)
    {
        options[currentOptionIndex].Select();
    }


    /* ================= INPUT ================= */

    private void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (options == null || options.Length == 0) return;

        float value = ctx.ReadValue<float>();
        if (Mathf.Abs(value) < 0.5f)
            return;

        int direction = value > 0 ? 1 : -1;

        ChangeOption(direction);
    }

    private void OnCancel(InputAction.CallbackContext ctx)
    {
        if (isTransitioning) return;

        PageExit();
    }

    /* ================= LOGIC ================= */

    private void ChangeOption(int direction)
    {
        options[currentOptionIndex].Hover(false);

        currentOptionIndex -= direction;

        // Wrap around
        if (currentOptionIndex < 0)
            currentOptionIndex = options.Length - 1;
        else if (currentOptionIndex >= options.Length)
            currentOptionIndex = 0;

        if (options[currentOptionIndex].isDisabled)
        {
            currentOptionIndex -= direction;
        }

        UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (options == null || options.Length == 0) return;
        for (int i = 0; i < options.Length; i++)
        {
            if (i != currentOptionIndex)
                options[i].Hover(false);
        }
        options[currentOptionIndex].Hover(true);
    }

    public void PlayPageTransitionSound()
    {
        if (pageTransitionClip != null)
        {
            audioSource.PlayOneShot(pageTransitionClip);
        }
    }

    public void PlayPageExitSound()
    {
        if (pageExitClip != null)
        {
            audioSource.PlayOneShot(pageExitClip);
        }
    }

    /* ================= PAGE ================= */

    public virtual void PageEnter()
    {
        if (isTransitioning) return;

        isTransitioning = true;
        gameObject.SetActive(true);
        StartCoroutine(FadeIn(() => isTransitioning = false));
        StartCoroutine(ChangeContainerWidth());
        currentOptionIndex = 0;
        UpdateSelection();
    }

    public void TransitionTo(UI_Page uiChild)
    {
        if (isTransitioning) return;
        uiChild.parentPage = this;

        isTransitioning = true;
        uiChild.PageEnter();
        PlayPageTransitionSound();
        StartCoroutine(FadeOut(() =>
        {
            gameObject.SetActive(false);
            isTransitioning = false;
        }));
    }

    public virtual void PageExit()
    {
        if (isTransitioning) return;    

        isTransitioning = true;
        PlayPageExitSound();
        if (parentPage)
        {
            parentPage.gameObject.SetActive(true);
            parentPage.StartCoroutine(parentPage.FadeIn(() => isTransitioning = false));
            parentPage.StartCoroutine(parentPage.ChangeContainerWidth());
            StartCoroutine(FadeOut(() => gameObject.SetActive(false)));
        }
        else
        {
            MenuManager manager = GetComponentInParent<MenuManager>();
            StartCoroutine(FadeOut(() =>
            {
                manager.HideMenu();
                isTransitioning = false;
            }));
        }
        canvasGroup.alpha = 1;
    }

    private IEnumerator FadeIn(System.Action onComplete)
    {
        canvasGroup.alpha = 0;
        float duration = 0.5f; // Duration for canvas group alpha fade
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            canvasGroup.alpha = t;
            yield return null;
        }

        canvasGroup.alpha = 1;
        onComplete?.Invoke();
    }

    private IEnumerator ChangeContainerWidth()
    {
        float widthChangeDuration = 0.2f; // Faster duration for container width change
        float elapsed = 0;

        // Store the initial and target anchor values
        float initialLeft = containerMask.padding.x;
        float initialRight = containerMask.padding.z;
        float targetLeft = targetPadding;
        float targetRight = targetPadding;

        while (elapsed < widthChangeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / widthChangeDuration);

            containerMask.padding = new Vector4(
                Mathf.Lerp(initialLeft, targetLeft, t),
                containerMask.padding.y,
                Mathf.Lerp(initialRight, targetRight, t),
                containerMask.padding.w
            );
            yield return null;
        }

        containerMask.padding = new Vector4(targetLeft, containerMask.padding.y, targetRight, containerMask.padding.w);
    }

    private IEnumerator FadeOut(System.Action onComplete)
    {
        canvasGroup.alpha = 1;
        float duration = 0.5f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = 1 - Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = 0;
        onComplete?.Invoke();
    }
}
