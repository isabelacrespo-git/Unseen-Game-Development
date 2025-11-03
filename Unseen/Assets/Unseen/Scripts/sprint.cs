using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR;

public class Sprint : MonoBehaviour
{
    public ContinuousMoveProvider moveProvider;
    public float normalSpeed = 2f;
    public float sprintSpeed = 5f;

    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRechargeRate = 15f;

    private float currentStamina;
    private InputDevice leftController;

    void Start()
    {
        currentStamina = maxStamina;
    }

    void Update()
    {
        if (!leftController.isValid)
        {
            leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        }

        bool wantsToSprint = false;
        leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out wantsToSprint);

        // Can only sprint if we have stamina
        if (wantsToSprint && currentStamina > 0)
        {
            moveProvider.moveSpeed = sprintSpeed;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
        }
        else
        {
            moveProvider.moveSpeed = normalSpeed;

            // Recharge stamina when not sprinting
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRechargeRate * Time.deltaTime;
                currentStamina = Mathf.Min(currentStamina, maxStamina);
            }
        }
    }

    public float GetStaminaPercent()
    {
        return currentStamina / maxStamina;
    }
}