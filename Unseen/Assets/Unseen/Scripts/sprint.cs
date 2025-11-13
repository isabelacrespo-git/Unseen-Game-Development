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
    public float rechargeDelay = 1f;

    private float currentStamina;
    private InputDevice leftController;
    private float timeSinceLastSprint = 0f;

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

        if (wantsToSprint && currentStamina > 0)
        {
            moveProvider.moveSpeed = sprintSpeed;
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(currentStamina, 0);
            timeSinceLastSprint = 0f;
        }
        else
        {
            moveProvider.moveSpeed = normalSpeed;
            timeSinceLastSprint += Time.deltaTime;

            // Only recharge after delay
            if (timeSinceLastSprint >= rechargeDelay && currentStamina < maxStamina)
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