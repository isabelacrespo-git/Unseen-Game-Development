using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;

public class MovementLockOnScene : MonoBehaviour
{
    [Tooltip("True to disable movement when this object starts, re-enabling when destroyed.")]
    public bool disableOnStart = true;
    [Tooltip("Disable sprint scripts in addition to movement providers.")]
    public bool affectSprint = true;

    void Start()
    {
        if (disableOnStart)
        {
            ToggleMovement(false);
        }
    }

    void OnDestroy()
    {
        ToggleMovement(true);
    }

    void ToggleMovement(bool enable)
    {
        foreach (var moveProvider in FindObjectsOfType<ContinuousMoveProvider>(true))
        {
            if (moveProvider != null)
                moveProvider.enabled = enable;
        }

        if (!affectSprint) return;

        foreach (var sprint in FindObjectsOfType<Sprint>(true))
        {
            if (sprint != null)
                sprint.enabled = enable;
        }
    }
}
