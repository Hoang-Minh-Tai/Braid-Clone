using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ButtonTutorial : MonoBehaviour
{
    private SaveManager saveManager;
    private TMPro.TextMeshProUGUI tutorialText;

    public string text1 = "Use the ARROW KEYS to move.";
    public float displayTime1 = 10f;
    public string text2 = "Press ESCAPE if you want the menu.";
    public float displayTime2 = 10f;
    public float fadeDuration = 3f; // Duration for fade in/out

    void Awake()
    {
        tutorialText = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        tutorialText.alpha = 0; // Ensure text is initially invisible
    }

    void Start()
    {
        saveManager = SaveManager.Instance;
        if (!saveManager.data.newGame)
        {
            Destroy(gameObject);
            return;
        }

        StartCoroutine(ShowTutorialSequence());
    }

    private IEnumerator ShowTutorialSequence()
    {
        // Show text1
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeTextIn(text1));
        yield return new WaitForSeconds(displayTime1);
        yield return StartCoroutine(FadeTextOut());

        // Show text2
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(FadeTextIn(text2));
        yield return new WaitForSeconds(displayTime2);
        yield return StartCoroutine(FadeTextOut());

        // Disable tutorial in PlayerPrefs
        saveManager.data.newGame = false;
        saveManager.SaveData();
    }

    private IEnumerator FadeTextIn(string text)
    {
        tutorialText.text = text;
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            tutorialText.alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            yield return null;
        }
    }

    private IEnumerator FadeTextOut()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            tutorialText.alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            yield return null;
        }
        tutorialText.text = ""; // Clear text after fading out
    }
}
