using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[Serializable]
public class DialogueState
{
    public int revealIndex;
    public float timer;
    public int[] charStartFrames;
    public float fade;
    public bool show;
    public float buttonFade;
    public bool buttonVisible;
}

public class Dialogue : MonoBehaviour
{
    [Header("Typing")]
    public float typingSpeed = 0.05f;
    public float colorFadeDuration = 0.4f;

    private TextMeshPro tmp;

    private float timer;
    public int revealIndex;
    private int[] charStartFrames;
    private float fade = 0;
    public float fadeDuration = 0.5f;
    private bool show = false;

    [SerializeField] private SpriteRenderer borderRenderer;
    [SerializeField] private SpriteRenderer backgroundRenderer;

    [Header("Button")]
    [SerializeField] private SpriteRenderer buttonBackground;
    [SerializeField] private TextMeshPro buttonText;
    [SerializeField] private SceneData_SO introScene;

    // ----------------------------
    // REWIND DATA
    // ----------------------------
    private List<DialogueState> dialogueStates = new();

    private void Awake()
    {
        tmp = GetComponentInChildren<TextMeshPro>();
        tmp.color = Color.clear;
    }

    void Start()
    {
        tmp.ForceMeshUpdate();

        int charCount = tmp.textInfo.characterCount;
        charStartFrames = new int[charCount];

        // Reveal all characters once
        tmp.maxVisibleCharacters = charCount;

        // Hide manually
        HideAllCharacters();
    }

    public void ShowDialogue()
    {
        show = true;
    }

    private void OnEnable()
    {
        if (GameEventManager.instance == null) return;
        InputManager.Instance.Input.Player.Jump.performed += EndScene;

        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.AddListener(EnableRewindMode);
        events.onRewindEnd.AddListener(DisableRewindMode);
    }

    private void OnDisable()
    {
        if (GameEventManager.instance == null) return;

        InputManager.Instance.Input.Player.Jump.performed -= EndScene;
        var events = GameEventManager.instance.generalEvent;
        events.onRewindStart.RemoveListener(EnableRewindMode);
        events.onRewindEnd.RemoveListener(DisableRewindMode);
    }

    private void EndScene(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        if (buttonVisible && buttonFade >= 1f)
        {
            SceneGameManager.Instance.LoadScene(introScene, 0);
        }
    }

    private void FixedUpdate()
    {
        // ----------------------------
        // REWIND MODE
        // ----------------------------
        if (RewindTimeManager.instance.rewinding)
        {
            if (dialogueStates.Count == 0) return;

            int index = RewindTimeManager.instance.currentFrameIndex;
            index = Mathf.Clamp(index, 0, dialogueStates.Count - 1);

            RestoreState(dialogueStates[index]);
            AnimateCharacters();
            return;
        }

        // ----------------------------
        // NORMAL MODE
        // ----------------------------
        dialogueStates.Add(RecordState());
        if (!show) return;

        timer += Time.fixedDeltaTime;

        if (revealIndex < charStartFrames.Length && timer >= typingSpeed)
        {
            timer = 0f;
            charStartFrames[revealIndex] = RewindTimeManager.instance.currentFrameIndex;
            revealIndex++;
        }
        fade = Mathf.MoveTowards(fade, 1f, Time.fixedDeltaTime / fadeDuration);
        AnimateCharacters();
        ApplyFade();
        CheckDialogueCompletion();
    }

    // ----------------------------
    // BUTTON FADE LOGIC
    // ----------------------------
    private float buttonFade = 0f;
    private bool buttonVisible = false;

    private void UpdateButtonFade()
    {
        if (buttonBackground != null && buttonText != null)
        {
            Color bgColor = buttonBackground.color;
            bgColor.a = buttonFade;
            buttonBackground.color = bgColor;

            Color textColor = buttonText.color;
            textColor.a = buttonFade;
            buttonText.color = textColor;
        }
    }

    private void CheckDialogueCompletion()
    {
        if (revealIndex >= charStartFrames.Length && !buttonVisible)
        {
            buttonVisible = true;
        }

        if (buttonVisible)
        {
            buttonFade = Mathf.MoveTowards(buttonFade, 1f, Time.fixedDeltaTime / fadeDuration);
            UpdateButtonFade();
        }
    }

    // ----------------------------
    // REWIND CONTROL (UPDATED)
    // ----------------------------
    private void EnableRewindMode(int rewindSpeed)
    {
        // Dialogue driven manually, nothing to freeze
    }

    private void DisableRewindMode()
    {
        int index = RewindTimeManager.instance.currentFrameIndex;

        // Delete future timeline
        if (index < dialogueStates.Count - 1)
            dialogueStates.RemoveRange(
                index + 1,
                dialogueStates.Count - index - 1
            );
    }

    // ----------------------------
    // STATE SYSTEM
    // ----------------------------
    private DialogueState RecordState()
    {
        return new DialogueState
        {
            fade = this.fade,
            revealIndex = this.revealIndex,
            timer = this.timer,
            charStartFrames = (int[])this.charStartFrames.Clone(),
            show = this.show,
            buttonFade = this.buttonFade,
            buttonVisible = this.buttonVisible
        };
    }

    private void RestoreState(DialogueState state)
    {
        revealIndex = state.revealIndex;
        fade = state.fade;

        timer = state.timer;
        show = state.show;
        charStartFrames = (int[])state.charStartFrames.Clone();
        buttonFade = state.buttonFade;
        buttonVisible = state.buttonVisible;
        ApplyFade();
        UpdateButtonFade();
    }

    // ----------------------------
    // TMP VISUALS
    // ----------------------------
    private void HideAllCharacters()
    {
        var textInfo = tmp.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            var colors = textInfo.meshInfo[meshIndex].colors32;

            for (int v = 0; v < 4; v++)
                colors[vertexIndex + v] = new Color32(0, 0, 0, 0);
        }

        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void AnimateCharacters()
    {
        if (!show) return;
        var textInfo = tmp.textInfo;
        int currentFrame = RewindTimeManager.instance.currentFrameIndex;

        for (int i = 0; i < revealIndex; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int startFrame = charStartFrames[i];

            float elapsedTime =
                (currentFrame - startFrame) * Time.fixedDeltaTime;

            float t = Mathf.Clamp01(elapsedTime / colorFadeDuration);

            Color32 color = Color.Lerp(Color.white, Color.black, t);
            color.a = (byte)(255 * t);

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            var colors = textInfo.meshInfo[meshIndex].colors32;

            for (int v = 0; v < 4; v++)
                colors[vertexIndex + v] = color;
        }
        for (int i = revealIndex; i < textInfo.characterCount; i++)
        {
            var charInfo = textInfo.characterInfo[i];
            if (!charInfo.isVisible) continue;

            int meshIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            var colors = textInfo.meshInfo[meshIndex].colors32;

            for (int v = 0; v < 4; v++)
                colors[vertexIndex + v] = new Color32(0, 0, 0, 0);
        }

        tmp.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void ApplyFade()
    {
        if (!show) return;
        if (borderRenderer != null)
        {
            Color c = borderRenderer.color;
            c.a = fade;
            borderRenderer.color = c;
        }

        if (backgroundRenderer != null)
        {
            Color c = backgroundRenderer.color;
            c.a = fade;
            backgroundRenderer.color = c;
        }
    }
}
