using UnityEngine;


public class Balloon : MonoBehaviour
{
    public float floatForce = 5f;
    public float bobAmount = 0.5f;
    public float bobSpeed = 1f;
    public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable stringGrab;

    public bool isSpecialBalloon = false;
    public AudioSource musicSourceToStop;
    public AudioSource staticSourceToStart;
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
            // Check if we hit something above (ceiling)
            if (!Physics.Raycast(transform.position, Vector3.up, 0.5f))
            {
                rb.AddForce(Vector3.up * floatForce, ForceMode.Force);

                Vector3 bobOffset = new Vector3(
                    Mathf.PerlinNoise(Time.time * 0.5f, 0) * bobAmount,
                    Mathf.Sin(Time.time * bobSpeed) * bobAmount,
                    Mathf.PerlinNoise(0, Time.time * 0.5f) * bobAmount
                );

                rb.AddForce(bobOffset, ForceMode.Force);
            }
            else
            {
                // At ceiling - stop all movement
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }

        Quaternion targetRotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    public void Pop()
    {
        if (isPopped) return;
        isPopped = true;

        if (isSpecialBalloon)
        {
            if (musicSourceToStop != null)
            {
                musicSourceToStop.Stop();
            }

            if (staticSourceToStart != null)
            {
                staticSourceToStart.Play();
            }

            if (notePrefab != null)
            {
                Instantiate(notePrefab, transform.position, Quaternion.identity);
            }
        }

        Destroy(gameObject);
    }
}