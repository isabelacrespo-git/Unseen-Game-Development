using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutSceneManager : MonoBehaviour
{
    [Header("Scene Flow")]
    public string nextSceneName;
    public bool playOnStart = true;

    [Header("Fader Settings")]
    public ScreenFader screenFader;
    public float fadeDuration = 1f;
    public float blackScreenHold = 0.35f;

    bool _isPlaying;

    void Start()
    {
        if (playOnStart)
        {
            StartSequence();
        }
    }

    public void StartSequence()
    {
        if (_isPlaying) return;
        if (string.IsNullOrEmpty(nextSceneName))
        {
            Debug.LogWarning("CutSceneManager: Next scene name is empty.");
            return;
        }

        StartCoroutine(RunCutScene());
    }

    IEnumerator RunCutScene()
    {
        _isPlaying = true;

        ScreenFader fader = EnsureFader();
        if (fader == null)
        {
            _isPlaying = false;
            yield break;
        }

        DontDestroyOnLoad(fader.gameObject);

        yield return StartCoroutine(fader.FadeIn(fadeDuration));
        yield return new WaitForSeconds(blackScreenHold);

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(nextSceneName);
        if (loadOp == null)
        {
            Debug.LogError($"CutSceneManager: Failed to load scene '{nextSceneName}'.");
            yield break;
        }

        while (!loadOp.isDone)
        {
            yield return null;
        }

        yield return StartCoroutine(fader.FadeOut(fadeDuration));

        Destroy(fader.gameObject);
        _isPlaying = false;
    }

    ScreenFader EnsureFader()
    {
        if (screenFader != null) return screenFader;

        screenFader = FindObjectOfType<ScreenFader>();
        if (screenFader == null)
        {
            Debug.LogError("CutSceneManager: No ScreenFader found in the scene.");
        }
        return screenFader;
    }
}
