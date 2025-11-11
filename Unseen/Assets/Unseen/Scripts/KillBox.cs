using UnityEngine;

[RequireComponent(typeof(Collider))]
public class KillBox : MonoBehaviour
{
    private Collider myCollider;
    private RespawnManager manager;

    void Awake()
    {
        myCollider = GetComponent<Collider>();
        myCollider.isTrigger = true;
        manager = FindObjectOfType<RespawnManager>();
    }

    void Update()
    {
        if (manager == null || manager.playerCamera == null) return;

        Vector3 playerPos = manager.playerCamera.position;

        if (myCollider.bounds.Contains(playerPos))
        {
            Debug.Log("Player inside KillBox Respawning...");
            manager.RespawnPlayer();
        }
    }
}
