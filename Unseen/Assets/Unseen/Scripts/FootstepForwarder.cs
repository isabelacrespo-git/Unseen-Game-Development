using UnityEngine;

public class FootstepForwarder : MonoBehaviour
{
    private WeepingAngel angelScript;

    void Start()
    {
        // Find the WeepingAngel script on the parent
        angelScript = GetComponentInParent<WeepingAngel>();
        
        if (angelScript == null)
        {
            Debug.LogError("FootstepForwarder: Could not find WeepingAngel script on parent!");
        }
    }

    // Called by Animation Event
    public void OnFootstep()
    {
        if (angelScript != null)
        {
            angelScript.OnFootstep();
        }
    }
}