using UnityEngine;


public class Balloon : MonoBehaviour
{
    public float floatForce = 5f;
    public float bobAmount = 0.5f;
    public float bobSpeed = 1f;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable stringGrab;

    public bool isSpecialBalloon = false;
    public AudioSource musicSource;
    public AudioClip staticSound;
    public GameObject notePrefab;

    private Rigidbody rb;
    private bool isPopped = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.linearDamping = 2f;
        rb.angularDamping = 2f;
    }

    void FixedUpdate()
    {
        if (isPopped) return;

        if (!stringGrab.isSelected)
        {
            rb.AddForce(Vector3.up * floatForce, ForceMode.Force);

            Vector3 bobOffset = new Vector3(
                Mathf.PerlinNoise(Time.time * 0.5f, 0) * bobAmount,
                Mathf.Sin(Time.time * bobSpeed) * bobAmount,
                Mathf.PerlinNoise(0, Time.time * 0.5f) * bobAmount
            );

            rb.AddForce(bobOffset, ForceMode.Force);
        }

        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void Pop()
    {
        if (isPopped) return;
        isPopped = true;

        if (isSpecialBalloon && musicSource != null)
        {
            musicSource.Stop();
            musicSource.clip = staticSound;
            musicSource.Play();
        }

        if (notePrefab != null)
        {
            Instantiate(notePrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }
}