using UnityEngine;

public class CodeDoor : MonoBehaviour
{
    public NumberDisplay[] numberDisplays;
    public Transform doorTransform;
    public Vector3 openPosition;
    public float openSpeed = 2f;

    private bool isUnlocked = false;
    private Vector3 closedPosition;

    void Start()
    {
        closedPosition = doorTransform.localPosition;
    }

    void Update()
    {
        if (isUnlocked)
        {
            doorTransform.position = Vector3.Lerp(
                doorTransform.position,
                openPosition,
                Time.deltaTime * openSpeed
            );
        }
    }

    public void CheckAllNumbers()
    {
        bool allCorrect = true;

        foreach (NumberDisplay display in numberDisplays)
        {
            if (!display.IsCorrect())
            {
                allCorrect = false;
                break;
            }
        }

        if (allCorrect && !isUnlocked)
        {
            isUnlocked = true;
            Debug.Log("Code correct! Door unlocked!");
        }
    }
}