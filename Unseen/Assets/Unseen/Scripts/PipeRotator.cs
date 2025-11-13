using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class PipeRotator : MonoBehaviour
{
    public enum Axis { X, Y, Z }

    [Header("Rotation Settings")]
    public Axis rotationAxis = Axis.Y;
    public float rotationStep = 90f;

    private XRGrabInteractable grab;
    private float accumulatedAngle = 0f;   // tracks total rotation

    void Awake()
    {
        grab = GetComponent<XRGrabInteractable>();
        grab.selectEntered.AddListener(OnGrab);
    }

    void OnGrab(SelectEnterEventArgs args)
    {
        // Pick a consistent local axis regardless of model tilt
        Vector3 localAxis = rotationAxis switch
        {
            Axis.X => transform.right,    // local X axis
            Axis.Y => transform.up,       // local Y axis
            Axis.Z => transform.forward,  // local Z axis
            _ => transform.up
        };

        // Apply rotation using quaternion math
        transform.rotation = Quaternion.AngleAxis(rotationStep, localAxis) * transform.rotation;

        // Track accumulated rotation for debug / puzzle logic
        accumulatedAngle = (accumulatedAngle + rotationStep) % 360f;
        if (accumulatedAngle < 0) accumulatedAngle += 360f;

        // Debug.Log($"{name} rotated {rotationStep}° around {rotationAxis} (total {accumulatedAngle})");
    }

    void OnDestroy()
    {
        grab.selectEntered.RemoveListener(OnGrab);
    }
}
