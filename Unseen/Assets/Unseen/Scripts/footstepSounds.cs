using UnityEngine;

public class FootstepAudio : MonoBehaviour
{
    public AudioClip[] footstepClips;
    public float stepDistance = 1.8f; // meters per step
    public Transform audioSourceTransform; // assign FootstepAudioSource object

    private AudioSource audioSource;
    private Vector3 lastPosition;
    private float distanceAccumulator;

    void Start()
    {
        if (audioSourceTransform == null)
        {
            Debug.LogError("FootstepAudio: audioSourceTransform not assigned!");
            enabled = false;
            return;
        }

        audioSource = audioSourceTransform.GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("FootstepAudio: No AudioSource found on the assigned transform.");
            enabled = false;
            return;
        }

        audioSource.spatialBlend = 1f;
        audioSource.reverbZoneMix = 1f;
        audioSource.playOnAwake = false;

        if (Camera.main != null)
            lastPosition = Camera.main.transform.position;
    }

    void Update()
    {
        if (Camera.main == null) return;

        Vector3 camPos = Camera.main.transform.position;
        float moved = Vector3.Distance(camPos, lastPosition);
        distanceAccumulator += moved;

        if (distanceAccumulator >= stepDistance)
        {
            PlayFootstep(camPos);
            distanceAccumulator = 0f;
        }

        lastPosition = camPos;
    }

    void PlayFootstep(Vector3 position)
    {
        if (footstepClips.Length == 0) return;

        int index = Random.Range(0, footstepClips.Length);
        audioSource.transform.position = new Vector3(
            position.x, position.y - 1f, position.z
        ); // place sound slightly below head height
        audioSource.clip = footstepClips[index];
        audioSource.pitch = Random.Range(0.95f, 1.05f);
        audioSource.volume = 0.7f;
        audioSource.Play();
    }
}
