using UnityEngine;

public class Door : MonoBehaviour
{
    public Transform doorTransform;
    public Vector3 openPosition;
    public float openSpeed = 2f;

    private bool isUnlocked = false;
    private Vector3 closedPosition;

    void Start()
    {
        closedPosition = doorTransform.position;
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

    public void UnlockDoor(GameObject key)
    {
        if (key.CompareTag(gameObject.tag))
        {
            isUnlocked = true;
            Debug.Log("Door unlocked!");
        }
        else
        {
            Debug.Log("Wrong key!");
        }
    }
}