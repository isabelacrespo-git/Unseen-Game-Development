using UnityEngine;
using System;
using System.Collections;

public class PartygoerEncounter : MonoBehaviour
{
    [Header("Balloon")]
    public Transform balloonRoot;         // object that scales up
    public float maxScale = 2.5f;         // how big before pop
    public float timeToFull = 10f;        // seconds from spawn to pop if never seen
    public AnimationCurve growthCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("Flashlight interaction")]
    public float requiredLightTime = 1.5f; // how long flashlight must stay on it
    public float shrinkSpeed = 0.5f;       // how quickly balloon deflates when lit

    [Header("FX")]
    public AudioSource audioSource;
    public AudioClip spawnStinger;
    public AudioClip balloonPopSound;
    public AudioClip vanishSound;

    [Header("Debug / Indicators")]
    public bool enableDebugLogs = true;
    public GameObject flashlightHitIndicator;

    Transform _player;
    Action<bool> _onFinished; // callback to manager

    float _growTimer;
    float _litTimer;
    bool _isActive = false;
    bool _isPopping = false;
    bool _isLit = false;

    Vector3 _initialScale;
    bool _wasLitLastFrame;

    void Awake()
    {
        if (balloonRoot == null)
            balloonRoot = transform; // fallback

        _initialScale = balloonRoot.localScale;

        if (flashlightHitIndicator != null)
            flashlightHitIndicator.SetActive(false);

        _wasLitLastFrame = false;
    }

    public void BeginEncounter(Transform player, Action<bool> onFinished)
    {
        _player = player;
        _onFinished = onFinished;
        _growTimer = 0f;
        _litTimer = 0f;
        _isActive = true;
        _isPopping = false;
        _isLit = false;

        if (audioSource != null && spawnStinger != null)
            audioSource.PlayOneShot(spawnStinger);

        if (flashlightHitIndicator != null)
            flashlightHitIndicator.SetActive(false);

        _wasLitLastFrame = false;
        LogDebug("Encounter started.");
    }

    void Update()
    {
        if (!_isActive) return;

        // always look at player on spawn (optional: you could lerp, etc.)
        if (_player != null)
        {
            Vector3 lookPos = _player.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        }

        float dt = Time.deltaTime;

        if (!_isLit)
        {
            _growTimer += dt;
        }
        else
        {
            // shrink / pause growth when lit
            _growTimer -= dt * shrinkSpeed;
            _growTimer = Mathf.Max(_growTimer, 0f);

            _litTimer += dt;
            if (_litTimer >= requiredLightTime)
            {
                // cleansed successfully
                ResolveEncounter(true);
                return;
            }
        }

        float t = Mathf.Clamp01(_growTimer / timeToFull);
        float curveT = growthCurve.Evaluate(t);
        balloonRoot.localScale = _initialScale * Mathf.Lerp(1f, maxScale, curveT);

        if (!_isPopping && t >= 1f)
        {
            // balloon pops, bad outcome
            PopBalloon();
            ResolveEncounter(false);
        }

        if (_isLit && !_wasLitLastFrame)
        {
            LogDebug("Flashlight contact established.");
        }
        else if (!_isLit && _wasLitLastFrame)
        {
            LogDebug("Flashlight contact lost.");
        }

        if (flashlightHitIndicator != null)
            flashlightHitIndicator.SetActive(_isLit);

        // reset lit timer if no longer lit
        if (!_isLit)
            _litTimer = 0f;

        _wasLitLastFrame = _isLit;

        // reset "lit flag" for next frame - FlashlightRaycaster will set it again
        _isLit = false;
    }

    void PopBalloon()
    {
        LogDebug("Balloon pop triggered. Initiating jumpscare.");
        _isPopping = true;
        HideVisuals(false);
        PlayOneShot(balloonPopSound);
        float delay = balloonPopSound != null ? balloonPopSound.length : 0.2f;
        JumpscareManager.Instance?.TriggerJumpscare(gameObject, delay);
    }

    void ResolveEncounter(bool success)
    {
        _isActive = false;
        LogDebug($"Encounter resolved. success={success}");

        if (audioSource != null)
        {
            audioSource.loop = false; // Stop looping urgent sound
            if (success)
            {
                audioSource.Stop();
            }
        }

        if (flashlightHitIndicator != null)
            flashlightHitIndicator.SetActive(false);

        if (success)
        {
            PlayOneShot(vanishSound);
            HideVisuals(true);
            float cleanupDelay = vanishSound != null ? Mathf.Max(vanishSound.length, 1f) : 1f;
            Destroy(gameObject, cleanupDelay);
        }

        _onFinished?.Invoke(success);
    }

    /// <summary>
    /// Called externally when flashlight is hitting this entity.
    /// </summary>
    public void SetLitByFlashlight()
    {
        _isLit = true;
        LogDebugVerbose("Flashlight hit registered this frame.");
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
        {
            Debug.Log($"[PartygoerEncounter] {message}", this);
        }
    }

    void LogDebugVerbose(string message)
    {
        if (enableDebugLogs && Debug.isDebugBuild)
        {
            Debug.Log($"[PartygoerEncounter] {message}", this);
        }
    }

    void HideVisuals(bool hideEntireObject)
    {
        if (balloonRoot != null)
        {
            foreach (var renderer in balloonRoot.GetComponentsInChildren<Renderer>())
                renderer.enabled = false;
            foreach (var collider in balloonRoot.GetComponentsInChildren<Collider>())
                collider.enabled = false;
        }

        if (hideEntireObject)
        {
            foreach (var renderer in GetComponentsInChildren<Renderer>())
                renderer.enabled = false;
            foreach (var collider in GetComponentsInChildren<Collider>())
                collider.enabled = false;
        }
    }

    void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null) return;

        audioSource.Stop();
        audioSource.PlayOneShot(clip);
    }

    IEnumerator BeginJumpscareAfterPop()
    {
        float wait = balloonPopSound != null ? balloonPopSound.length : 0.2f;
        yield return new WaitForSeconds(wait);
        HideVisuals(false);
        JumpscareManager.Instance?.TriggerJumpscare(gameObject);
    }
}
