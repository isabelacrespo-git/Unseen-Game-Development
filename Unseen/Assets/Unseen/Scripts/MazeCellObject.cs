using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MazeCellObject : MonoBehaviour
{
    [SerializeField] GameObject topWall;
    [SerializeField] GameObject bottomWall;
    [SerializeField] GameObject rightWall;
    [SerializeField] GameObject leftWall;
    [SerializeField] GameObject floorObject;
    [SerializeField] Transform floorAnchor;

    public void Init(bool top, bool bottom, bool right, bool left)
    {
        topWall.SetActive(top);
        bottomWall.SetActive(bottom);
        rightWall.SetActive(right);
        leftWall.SetActive(left);
    }

    public GameObject ReplaceFloorWith(GameObject replacementPrefab, float heightOffset = 0f, Vector3 rotationEulerOffset = default)
    {
        if (replacementPrefab == null)
            return null;

        if (floorObject != null)
        {
            floorObject.SetActive(false);
        }

        Vector3 spawnPos = floorAnchor != null ? floorAnchor.position : transform.position;
        Quaternion spawnRot = floorAnchor != null ? floorAnchor.rotation : transform.rotation;
        if (rotationEulerOffset != Vector3.zero)
        {
            spawnRot *= Quaternion.Euler(rotationEulerOffset);
        }
        spawnPos += Vector3.up * heightOffset;

        return Instantiate(replacementPrefab, spawnPos, spawnRot, transform);
    }
}
