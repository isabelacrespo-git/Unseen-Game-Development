using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenSceneLoader : MonoBehaviour
{
    [Tooltip("Optional override. If left empty, loadSceneName passed into methods will be used.")]
    public string defaultSceneName;
    public ScreenFader screenFader;
    public float fadeDuration = 1f;
    public float blackHoldDuration = 0.25f;

    bool _loading;

    public void LoadScene(string sceneName)
    {
        if (_loading) return;
        string target = string.IsNullOrEmpty(sceneName) ? defaultSceneName : sceneName;
        if (string.IsNullOrEmpty(target))
        {
            Debug.LogWarning("ScreenSceneLoader: No scene specified.");
            return;
        }

        StartCoroutine(LoadSceneRoutine(target));
    }

    public void LoadDefaultScene()
    {
        LoadScene(defaultSceneName);
    }

    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator LoadSceneRoutine(string sceneName)
    {
        _loading = true;

        ScreenFader fader = screenFader != null ? screenFader : ScreenFader.Instance;
        if (fader == null)
        {
            SceneManager.LoadScene(sceneName);
            yield break;
        }

        yield return StartCoroutine(fader.FadeIn(fadeDuration));
        yield return new WaitForSeconds(blackHoldDuration);

        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName);
        while (!loadOp.isDone)
            yield return null;

        yield return StartCoroutine(fader.FadeOut(fadeDuration));
        _loading = false;
    }
}
