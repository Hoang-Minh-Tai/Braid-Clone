using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CloudText : MonoBehaviour
{
    [SerializeField] private CloudTextData_SO cloudTextData;
    [SerializeField] private TextMeshPro textOut;
    [SerializeField] private TextMeshPro textIn;
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine fadeInCo;
    private Coroutine fadeOutCo;


    private void Start()
    {
        textOut.text = "";
        textIn.text = "";

        textOut.alpha = 0f;
        textIn.alpha = 0f;
    }

    public void Show(int index)
    {
        if (index < 0 || index >= cloudTextData.bodyTexts.Length)
            return;

        FadeIn(cloudTextData.bodyTexts[index]);
    }

    public void Hide()
    {
        textOut.text = textIn.text;
        textOut.alpha = textIn.alpha;
        textIn.text = "";

        if (fadeOutCo != null)
            StopCoroutine(fadeOutCo);

        fadeOutCo = StartCoroutine(FadeText(textOut, textOut.alpha, 0f));
    }

    private void FadeIn(string newText)
    {
        textIn.text = newText;
        textIn.alpha = 0f;

        if (fadeInCo != null)
            StopCoroutine(fadeInCo);

        fadeInCo = StartCoroutine(FadeText(textIn, 0f, 1f));
    }

    private IEnumerator FadeText(TextMeshPro textMesh, float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / fadeDuration);
            textMesh.alpha = alpha;
            yield return null;
        }

        textMesh.alpha = endAlpha;
    }
}
