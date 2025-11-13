using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VatController : MonoBehaviour
{
    [Header("References")]
    public Transform water;
    public Transform key;
    public XRGrabInteractable hatch;
    public XRGrabInteractable keyGrab;


    [Header("Settings")]
    public float riseHeight = 2f;
    public float riseSpeed = 0.5f;

    private Vector3 startPos;
    private Vector3 targetPos;
    private bool rising = false;
    private bool lockedKey = false;

    void Start()
    {
        startPos = water.localPosition;
        targetPos = startPos + Vector3.up * riseHeight;

        if (hatch != null)
            hatch.enabled = false;

        if (keyGrab != null)
            keyGrab.enabled = false;

        if (key.TryGetComponent(out Rigidbody rb))
        {
            rb.useGravity = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        lockedKey = true;
    }

    void Update()
    {
        if (!rising) return;

        // Move water upward
        water.localPosition = Vector3.MoveTowards(
            water.localPosition,
            targetPos,
            riseSpeed * Time.deltaTime
        );

        // Move key upward with water
        if (key != null && !lockedKey)
        {
            key.position = new Vector3(
                key.position.x,
                water.position.y + 0.1f,
                key.position.z
            );
        }

        // Stop when finished
        if (Vector3.Distance(water.localPosition, targetPos) < 0.01f)
        {
            rising = false;
            LockKeyInPlace();
        }
    }

    public void StartFilling()
    {
        rising = true;
        lockedKey = false;
    }

    private void LockKeyInPlace()
    {
        if (key.TryGetComponent(out Rigidbody rb))
        {
            rb.constraints = RigidbodyConstraints.FreezeAll;
            rb.useGravity = false;
        }

        // NOW allow grabbing
        if (keyGrab != null)
            keyGrab.enabled = true;

        lockedKey = true;
        Debug.Log("Water full — key unlocked and grabbable!");
    }

    public void OnKeyGrabbed(SelectEnterEventArgs args)
    {
        // Hide key when grabbed
        key.gameObject.SetActive(false);

        // Enable hatch
        if (hatch != null)
        {
            hatch.enabled = true;
            Debug.Log("Hatch is now unlocked and grabbable!");
        }
    }

    public void OnHatchGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Hatch grabbed! (Trigger scene change later)");
    }
}
