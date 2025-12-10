using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class CinematicController : MonoBehaviour
{
    public TextMeshProUGUI[] textElements;
    public float[] displayDurations;
    public float fadeDuration = 1f;
    public string nextSceneName = "MainGame";

    void Start()
    {
        // Set all text to invisible at start
        foreach (var text in textElements)
        {
            Color c = text.color;
            c.a = 0;
            text.color = c;
        }

        StartCoroutine(PlayCinematicWithDelay());
    }

    IEnumerator PlayCinematicWithDelay()
    {
        // Wait 15 seconds before starting
        yield return new WaitForSeconds(15f);

        yield return StartCoroutine(PlayCinematic());
    }

    IEnumerator PlayCinematic()
    {
        for (int i = 0; i < textElements.Length; i++)
        {
            // Fade in
            yield return StartCoroutine(FadeText(textElements[i], 0, 1, fadeDuration));

            // Hold
            float duration = (i < displayDurations.Length) ? displayDurations[i] : 3f;
            yield return new WaitForSeconds(duration);

            // Fade out
            yield return StartCoroutine(FadeText(textElements[i], 1, 0, fadeDuration));
        }

        // Load next scene
        SceneManager.LoadScene(nextSceneName);
    }

    IEnumerator FadeText(TextMeshProUGUI text, float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0;
        Color color = text.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            text.color = color;
            yield return null;
        }

        color.a = endAlpha;
        text.color = color;
    }
}