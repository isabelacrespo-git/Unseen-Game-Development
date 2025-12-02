using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class RespawnManager : MonoBehaviour
{
    public static RespawnManager Instance { get; private set; }

    [Header("References")]
    public Transform playerCamera;
    public ScreenFader screenFader;
    public string deathSceneName = "DeathScene";

    [Header("Timing")]
    public float fadeDuration = 1f;
    public float blackHoldDuration = 0.5f;

    [Header("Audio")]
    public AudioClip deathSceneIntroSound;

    AudioSource audioSource;
    bool isProcessing;
    bool movementLocked;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != deathSceneName)
        {
            WeepingAngel.ResetGlobalCaptureFlag();
        }

        if (movementLocked && scene.name != deathSceneName)
        {
            ToggleMovement(true);
            movementLocked = false;
        }
    }

    public void RespawnPlayer()
    {
        if (isProcessing) return;
        LockMovement();
        StartCoroutine(LoadDeathSceneRoutine());
    }

    void LockMovement()
    {
        if (movementLocked) return;
        ToggleMovement(false);
        movementLocked = true;
    }

    void ToggleMovement(bool enable)
    {
        foreach (var moveProvider in FindObjectsOfType<ContinuousMoveProvider>(true))
        {
            if (moveProvider != null)
                moveProvider.enabled = enable;
        }

        foreach (var sprint in FindObjectsOfType<Sprint>(true))
        {
            if (sprint != null)
                sprint.enabled = enable;
        }
    }

    IEnumerator LoadDeathSceneRoutine()
    {
        isProcessing = true;
        yield return StartCoroutine(FadeToScene(deathSceneName));
        isProcessing = false;
    }

    IEnumerator FadeToScene(string sceneName)
    {
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

        if (movementLocked)
            ToggleMovement(false);

        if (deathSceneIntroSound != null)
            AudioSource.PlayClipAtPoint(deathSceneIntroSound, Vector3.zero);

        yield return StartCoroutine(fader.FadeOut(fadeDuration));
    }
}
