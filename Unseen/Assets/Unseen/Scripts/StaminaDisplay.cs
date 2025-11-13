using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class StaminaDisplay : MonoBehaviour
{
    public Sprint sprintScript;
    public RectTransform staminaBarFill;
    public GameObject canvasObject;
    public InputActionProperty toggleAction;

    private bool wasButtonPressed = false;
    private bool isVisible = false;

    void Start()
    {
        canvasObject.SetActive(false);
        Debug.Log("StaminaDisplay started. Canvas is hidden.");
    }

    void Update()
    {
        bool isButtonPressed = toggleAction.action.IsPressed();

        if (isButtonPressed && !wasButtonPressed)
        {
            isVisible = !isVisible;
            canvasObject.SetActive(isVisible);
            Debug.Log("Button pressed! Canvas visible: " + isVisible);
        }
        wasButtonPressed = isButtonPressed;

        if (isVisible)
        {
            float staminaPercent = sprintScript.GetStaminaPercent();
            staminaBarFill.localScale = new Vector3(staminaPercent, 1, 1);
        }
    }
}