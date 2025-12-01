using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class JumpscareManager : MonoBehaviour
{
    public static JumpscareManager Instance;

    [Header("Timing")]
    public float blackoutDelay = 0.15f;
    public float timeBeforeJumpscare = 0.25f;
    public float postJumpscareDuration = 1.6f;

    [Header("Audio / Prefab")]
    public AudioClip blackoutSound;
    public AudioClip jumpscareSound;
    public GameObject jumpscarePrefab; // optional visual prefab to spawn at camera

    [Header("Camera / Animation")]
    public Camera jumpscareCamera;
    public Animator jumpscareAnimator;
    public string jumpscareTriggerName = "jumpscare";

    [Header("Game Over")]
    public bool loadGameOverOnFinish = true;
    public string gameOverSceneName = "GameOver";

    List<Light> _sceneLights = new List<Light>();
    List<bool> _previousLightStates = new List<bool>();

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    public void TriggerJumpscare(GameObject entity, float preBlackoutDelay = 0f)
    {
        StartCoroutine(JumpscareRoutine(entity, preBlackoutDelay));
    }

    IEnumerator JumpscareRoutine(GameObject entity, float preBlackoutDelay)
    {
        if (preBlackoutDelay > 0f)
            yield return new WaitForSeconds(preBlackoutDelay);

        Camera cam = Camera.main;
        if (cam == null && jumpscareCamera == null) yield break;

        bool playerCameraEnabled = cam != null && cam.enabled;
        bool playerCameraObjActive = cam != null && cam.gameObject.activeSelf;
        bool jumpscareCameraEnabled = jumpscareCamera != null && jumpscareCamera.enabled;
        bool jumpscareCameraObjActive = jumpscareCamera != null && jumpscareCamera.gameObject.activeSelf;

        // snapshot and disable lights
        _sceneLights.Clear();
        _previousLightStates.Clear();
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            _sceneLights.Add(l);
            _previousLightStates.Add(l.enabled);
            l.enabled = false;
        }

        // disable flashlight scripts
        foreach (var f in Object.FindObjectsByType<FlashlightRaycaster>(FindObjectsSortMode.None))
        {
            f.flashlightOn = false;
        }

        // play blackout sound near player
        Vector3 audioPos = cam != null ? cam.transform.position :
            (jumpscareCamera != null ? jumpscareCamera.transform.position : Vector3.zero);
        AudioSource blackoutSource = null;
        GameObject blackoutAudioObject = null;
        if (blackoutSound != null)
        {
            blackoutAudioObject = new GameObject("BlackoutAudioTemp");
            blackoutAudioObject.transform.position = audioPos;
            blackoutSource = blackoutAudioObject.AddComponent<AudioSource>();
            blackoutSource.clip = blackoutSound;
            blackoutSource.playOnAwake = false;
            blackoutSource.spatialBlend = 1f;
            blackoutSource.loop = false;
            blackoutSource.Play();
        }

        yield return new WaitForSeconds(blackoutDelay);

        // small delay then execute jumpscare: move entity in front of camera or spawn prefab
        if (entity != null && jumpscareCamera == null)
        {
            // place entity close to camera (in front)
            Transform e = entity.transform;
            Vector3 camPos = cam != null ? cam.transform.position : jumpscareCamera.transform.position;
            Vector3 camFwd = cam != null ? cam.transform.forward : jumpscareCamera.transform.forward;
            Vector3 targetPos = camPos + camFwd * 0.8f;
            e.position = targetPos;
            e.rotation = Quaternion.LookRotation((camPos - e.position).normalized, Vector3.up);
        }

        yield return new WaitForSeconds(timeBeforeJumpscare);

        if (jumpscareCamera != null)
        {
            jumpscareCamera.gameObject.SetActive(true);
            jumpscareCamera.enabled = true;

            if (cam != null)
            {
                cam.gameObject.SetActive(false);
                cam.enabled = false;
            }
        }

        if (jumpscareAnimator != null && !string.IsNullOrEmpty(jumpscareTriggerName))
        {
            jumpscareAnimator.ResetTrigger(jumpscareTriggerName);
            jumpscareAnimator.SetTrigger(jumpscareTriggerName);
        }

        // play jumpscare sound
        if (jumpscareSound != null)
        {
            if (blackoutSource != null)
            {
                blackoutSource.Stop();
                Destroy(blackoutAudioObject);
                blackoutSource = null;
            }

            Vector3 scarePos = jumpscareCamera != null ? jumpscareCamera.transform.position :
                (cam != null ? cam.transform.position : Vector3.zero);
            AudioSource.PlayClipAtPoint(jumpscareSound, scarePos);
        }

        // optional visual prefab
        if (jumpscarePrefab != null)
        {
            Camera spawnCam = jumpscareCamera != null ? jumpscareCamera : cam;
            if (spawnCam != null)
            {
                Instantiate(
                    jumpscarePrefab,
                    spawnCam.transform.position + spawnCam.transform.forward * 0.4f,
                    Quaternion.identity
                );
            }
        }

        // let jumpscare play out
        yield return new WaitForSeconds(postJumpscareDuration);

        if (loadGameOverOnFinish && !string.IsNullOrEmpty(gameOverSceneName))
        {
            if (blackoutSource != null)
            {
                blackoutSource.Stop();
                Destroy(blackoutAudioObject);
            }
            if (entity != null)
                Destroy(entity);
            SceneManager.LoadScene(gameOverSceneName);
            yield break;
        }

        if (blackoutSource != null)
        {
            blackoutSource.Stop();
            Destroy(blackoutAudioObject);
        }

        if (entity != null)
            Destroy(entity);

        // restore lights and flashlight
        for (int i = 0; i < _sceneLights.Count; i++)
        {
            if (_sceneLights[i] != null)
                _sceneLights[i].enabled = _previousLightStates[i];
        }

        foreach (var f in Object.FindObjectsByType<FlashlightRaycaster>(FindObjectsSortMode.None))
        {
            f.flashlightOn = true;
        }

        if (jumpscareCamera != null)
        {
            jumpscareCamera.gameObject.SetActive(jumpscareCameraObjActive);
            jumpscareCamera.enabled = jumpscareCameraEnabled;
        }

        if (cam != null)
        {
            cam.gameObject.SetActive(playerCameraObjActive);
            cam.enabled = playerCameraEnabled;
        }

        yield break;
    }
}
