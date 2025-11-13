using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class WeepingAngel : MonoBehaviour
{
    public NavMeshAgent ai;
    public Transform player;
    Vector3 dest;
    public Camera playerCam, jumpscareCam;
    public float aiSpeed, catchDistance, jumpscareTime;
    public string sceneAfterDeath;

    void Update()
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(playerCam);

        float distance = Vector3.Distance(ai.transform.position, player.position);

        if (GeometryUtility.TestPlanesAABB(planes, ai.GetComponent<Collider>().bounds))
        {
            ai.speed = 0;
            ai.SetDestination(ai.transform.position);
        }

        if (!GeometryUtility.TestPlanesAABB(planes, ai.GetComponent<Collider>().bounds))
        {
            ai.speed = aiSpeed;
            dest = player.position;
            ai.SetDestination(dest);
            if (distance <= catchDistance)
            {
                player.gameObject.SetActive(false);
                jumpscareCam.gameObject.SetActive(true);
                StartCoroutine(killPlayer());
            }
        }
    }
    IEnumerator killPlayer()
    {
        yield return new WaitForSeconds(jumpscareTime);
        SceneManager.LoadScene(sceneAfterDeath);
    }
}
