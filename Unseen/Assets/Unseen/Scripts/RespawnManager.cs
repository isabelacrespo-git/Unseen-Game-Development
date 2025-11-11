using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RespawnManager : MonoBehaviour
{
    [Header("References")]
    public Transform playerCamera;       // Assign Main Camera here
    public Transform respawnPoint;       // Assign your SpawnPoint
    private XROrigin xrOrigin;

    [Header("Optional Effects")]
    public AudioClip deathSound;
    public AudioClip respawnSound;
    public float respawnDelay = 1.5f;

    private AudioSource audioSource;
    private bool isRespawning = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        xrOrigin = FindObjectOfType<XROrigin>();
    }

    public void RespawnPlayer()
    {
        if (isRespawning) return;
        StartCoroutine(RespawnRoutine());
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        isRespawning = true;

        if (deathSound != null)
            audioSource.PlayOneShot(deathSound);

        yield return new WaitForSeconds(respawnDelay);

        if (xrOrigin != null && respawnPoint != null)
        {
            xrOrigin.MoveCameraToWorldLocation(respawnPoint.position);
            xrOrigin.transform.rotation = respawnPoint.rotation;
        }

        if (respawnSound != null)
            audioSource.PlayOneShot(respawnSound);

        isRespawning = false;
    }
}
