using UnityEngine;

public class PlayerTriggerRelay : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("KillBox"))
        {
            FindObjectOfType<RespawnManager>()?.RespawnPlayer();
        }
    }
}
