using UnityEngine;

public class Flashlight : MonoBehaviour
{
    [Header("References")]
    public Material lensMaterial;
    public Light flashlightLight;
    public AudioSource flashlightAudio;
    
    [Header("Audio Clips")]
    public AudioClip clickOnSound;
    public AudioClip clickOffSound;
    
    private bool isOn = false;
    private Material instanceMaterial; // Use instanced material

    void Start()
    {
        // Create material instance to avoid affecting other flashlights
        if (lensMaterial != null)
        {
            instanceMaterial = new Material(lensMaterial);
            var renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = instanceMaterial;
            }
        }
        
        // Validate references
        if (flashlightLight == null)
        {
            Debug.LogWarning("Flashlight: flashlightLight is not assigned!", this);
        }
        
        if (flashlightAudio == null)
        {
            Debug.LogWarning("Flashlight: flashlightAudio is not assigned!", this);
        }
        
        // Initialize to off state
        SetFlashlightState(false);
    }

    public void Toggle()
    {
        SetFlashlightState(!isOn);
    }

    public void LightOn()
    {
        SetFlashlightState(true);
    }

    public void LightOff()
    {
        SetFlashlightState(false);
    }

    private void SetFlashlightState(bool state)
    {
        isOn = state;
        
        // Toggle light
        if (flashlightLight != null)
        {
            flashlightLight.enabled = state;
        }
        
        // Toggle material emission
        if (instanceMaterial != null)
        {
            if (state)
                instanceMaterial.EnableKeyword("_EMISSION");
            else
                instanceMaterial.DisableKeyword("_EMISSION");
        }
        
        // Play sound effect
        if (flashlightAudio != null)
        {
            AudioClip clipToPlay = state ? clickOnSound : clickOffSound;
            if (clipToPlay != null)
            {
                flashlightAudio.PlayOneShot(clipToPlay);
            }
        }
    }

    void OnDestroy()
    {
        // Clean up instanced material
        if (instanceMaterial != null)
        {
            Destroy(instanceMaterial);
        }
    }
}

