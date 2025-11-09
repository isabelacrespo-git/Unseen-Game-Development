using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class KeySocket : MonoBehaviour
{
    public Door door;
    private XRSocketInteractor socket;

    void Start()
    {
        socket = GetComponent<XRSocketInteractor>();
        socket.selectEntered.AddListener(OnKeyInserted);
    }

    void OnKeyInserted(BaseInteractionEventArgs args)
    {
        GameObject key = args.interactableObject.transform.gameObject;
        door.UnlockDoor(key);
    }
}