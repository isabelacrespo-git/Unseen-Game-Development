using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class KeyLockedSceneSwitch : XRGrabInteractable
{
    [Tooltip("Scene to load when object is grabbed with key")]
    public string sceneToLoad;

    [Tooltip("The key GameObject that needs to be grabbed/collected")]
    public GameObject requiredKey;

    public ScreenFader screenFader;
    public float fadeDuration = 1f;
    public float blackHoldDuration = 0.25f;

    bool _loading;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (_loading || string.IsNullOrEmpty(sceneToLoad)) return;

        // Check if key has been collected (is inactive/disappeared)
        if (requiredKey != null && requiredKey.activeInHierarchy)
        {
            Debug.Log("You need to collect the key first!");
            return;
        }

        StartCoroutine(LoadSceneRoutine(sceneToLoad));
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