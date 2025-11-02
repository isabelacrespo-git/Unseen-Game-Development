using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR;

public class Sprint : MonoBehaviour
{
    public ContinuousMoveProvider moveProvider;
    public float normalSpeed = 2f;
    public float sprintSpeed = 5f;

    private InputDevice leftController;

    void Update()
    {
        // Get left controller if we don't have it
        if (!leftController.isValid)
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        // Check if left thumbstick is pressed
        bool isPressed = false;
        if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out isPressed) && isPressed)
        {
            moveProvider.moveSpeed = sprintSpeed;
        }
        else
        {
            moveProvider.moveSpeed = normalSpeed;
        }
    }
}