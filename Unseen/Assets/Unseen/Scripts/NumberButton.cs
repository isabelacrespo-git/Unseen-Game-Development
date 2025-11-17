using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class NumberButton : UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable
{
    public NumberDisplay numberDisplay;
    public bool isIncrement = true;
    public float pressDistance = 0.05f;

    private Vector3 startPosition;
    private bool isPressed = false;

    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.localPosition;
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);

        if (!isPressed)
        {
            isPressed = true;

            // Move button down
            transform.localPosition = startPosition - new Vector3(0, 0, pressDistance);

            // Change number
            if (isIncrement)
            {
                numberDisplay.IncrementNumber();
            }
            else
            {
                numberDisplay.DecrementNumber();
            }
        }
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);

        // Reset button
        transform.localPosition = startPosition;
        isPressed = false;
    }
}