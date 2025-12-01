using UnityEngine;

public class BalloonRig : MonoBehaviour
{
    [Header("References")]
    public Transform balloonRoot;        // parent object that sways (parent of balloon mesh + BalloonStringAttach)
    public Transform balloonStringAttach; // empty at bottom of balloon
    public Transform handStringAttach;    // empty inside the hand
    public Transform stringCapsule;       // the capsule mesh used as string

    [Header("Balloon sway")]
    public float swayAngle = 8f;       // tilt in degrees
    public float swaySpeed = 1.2f;     // side-to-side speed
    public float bobAmount = 0.05f;    // up/down distance in meters
    public float bobSpeed = 1.5f;      // up/down speed

    [Header("String length tweak")]
    public float lengthOffset = 0f;    // small extra length if needed

    Vector3 _balloonStartLocalPos;
    Quaternion _balloonStartLocalRot;
    Vector3 _stringInitialLocalScale;
    float _baseLength;
    float _phaseOffset;

    void Start()
    {
        if (!balloonRoot || !balloonStringAttach || !handStringAttach || !stringCapsule)
        {
            Debug.LogWarning("BalloonRig: assign balloonRoot, balloonStringAttach, handStringAttach, and stringCapsule.");
            enabled = false;
            return;
        }

        _balloonStartLocalPos = balloonRoot.localPosition;
        _balloonStartLocalRot = balloonRoot.localRotation;
        _stringInitialLocalScale = stringCapsule.localScale;

        // initial straight-line distance between hand and balloon attach points
        float dist = Vector3.Distance(handStringAttach.position, balloonStringAttach.position);
        _baseLength = dist > 0.0001f ? dist : 1f;

        _phaseOffset = Random.Range(0f, Mathf.PI * 2f);
    }

    void LateUpdate()
    {
        float t = Time.time + _phaseOffset;

        // --- 1) sway + bob the balloon root ---

        float sway = Mathf.Sin(t * swaySpeed) * swayAngle;
        float bob  = Mathf.Sin(t * bobSpeed)  * bobAmount;

        balloonRoot.localRotation =
            _balloonStartLocalRot * Quaternion.Euler(0f, 0f, sway);

        balloonRoot.localPosition =
            _balloonStartLocalPos + new Vector3(0f, bob, 0f);

        // --- 2) attach string between hand and balloon attach points ---

        Vector3 start = handStringAttach.position;
        Vector3 end   = balloonStringAttach.position;
        Vector3 dir   = end - start;
        float dist    = dir.magnitude;

        if (dist < 0.0001f)
            return;

        // position string halfway between both ends
        stringCapsule.position = start + dir * 0.5f;

        // orient string along the line (assuming capsule's long axis is Y)
        // if your capsule is along Z instead, use: stringCapsule.forward = dir.normalized;
        stringCapsule.up = dir.normalized;

        // scale string to match the current distance
        float scaleFactor = (dist + lengthOffset) / _baseLength;
        stringCapsule.localScale = new Vector3(
            _stringInitialLocalScale.x,
            _stringInitialLocalScale.y * scaleFactor,
            _stringInitialLocalScale.z
        );
    }
}



