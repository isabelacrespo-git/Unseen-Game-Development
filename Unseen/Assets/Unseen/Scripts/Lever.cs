using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Lever : XRGrabInteractable
{
    public NumberDisplay numberDisplay;
    public Transform leverHandle;
    public float rotationLimit = 45f;

    private bool wasInCenter = true;
    private float startRotation;

    protected override void Awake()
    {
        base.Awake();
        startRotation = leverHandle.localEulerAngles.x;

        // Force physics-based movement
        movementType = MovementType.VelocityTracking;
        trackPosition = false;
        trackRotation = true;
    }

    void Update()
    {
        if (isSelected)
        {
            float currentRotation = leverHandle.localEulerAngles.x;
            if (currentRotation > 180) currentRotation -= 360;

            bool inCenter = Mathf.Abs(currentRotation) < 10f;

            if (wasInCenter && currentRotation < -30f)
            {
                numberDisplay.DecrementNumber();
                wasInCenter = false;
            }
            else if (wasInCenter && currentRotation > 30f)
            {
                numberDisplay.IncrementNumber();
                wasInCenter = false;
            }
            else if (inCenter)
            {
                wasInCenter = true;
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        //leverHandle.localRotation = Quaternion.Euler(0, 0, 0);
        //wasInCenter = true;
    }
}