using UnityEngine;


public class Needle : UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable
{
    public Transform needleTip;

    void OnTriggerEnter(Collider other)
    {
        Balloon balloon = other.GetComponent<Balloon>();
        if (balloon != null)
        {
            balloon.Pop();
        }
    }
}