using UnityEngine;

public class FlashlightRaycaster : MonoBehaviour
{
    [Header("Detection")]
    public LayerMask partygoerMask;  // layer for Partygoer colliders
    public float maxDistance = 30f;
    public bool flashlightOn = true; // manual fallback toggle

    [Header("State Source")]
    public Flashlight flashlightSource; // optional reference to real flashlight component

    [Header("Debug")]
    public bool enableDebugLogs = true;
    public bool drawDebugRay = true;
    public Color missColor = Color.cyan;
    public Color hitColor = Color.green;

    [Header("Visual Feedback")]
    public Light hitFeedbackLight;
    public Color defaultLightColor = Color.white;
    public Color onTargetLightColor = Color.yellow;
    public float lightLerpSpeed = 12f;

    bool _wasHitting;

    void Awake()
    {
        if (hitFeedbackLight == null)
            hitFeedbackLight = GetComponentInChildren<Light>();

        if (hitFeedbackLight != null)
            hitFeedbackLight.color = defaultLightColor;
    }

    void Update()
    {
        bool isActive = flashlightSource != null ? flashlightSource.IsOn : flashlightOn;
        if (!isActive)
        {
            if (_wasHitting)
                LogDebug("Flashlight switched off, clearing contact.");
            _wasHitting = false;
            UpdateFeedback(false);
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);
        bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, maxDistance, partygoerMask);

        if (drawDebugRay)
        {
            Color rayColor = hitSomething ? hitColor : missColor;
            Debug.DrawRay(ray.origin, ray.direction * maxDistance, rayColor, Time.deltaTime);
        }

        if (hitSomething)
        {
            PartygoerEncounter enc = hit.collider.GetComponentInParent<PartygoerEncounter>();
            if (enc != null)
            {
                if (!_wasHitting)
                    LogDebug($"Hit partygoer '{enc.name}' at distance {hit.distance:F1}.");

                enc.SetLitByFlashlight();
                _wasHitting = true;
                UpdateFeedback(true);
                return;
            }
        }

        if (_wasHitting)
            LogDebug("Flashlight lost contact with partygoer.");

        _wasHitting = false;
        UpdateFeedback(false);
    }

    void UpdateFeedback(bool onTarget)
    {
        if (hitFeedbackLight == null) return;

        Color goal = onTarget ? onTargetLightColor : defaultLightColor;
        hitFeedbackLight.color = Color.Lerp(hitFeedbackLight.color, goal, Time.deltaTime * lightLerpSpeed);
    }

    void LogDebug(string message)
    {
        if (enableDebugLogs)
            Debug.Log($"[FlashlightRaycaster] {message}", this);
    }

    public void SetManualFlashlightState(bool state)
    {
        flashlightOn = state;
    }
}

