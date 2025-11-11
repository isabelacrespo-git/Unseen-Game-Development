using UnityEngine;
using System.Collections.Generic;

public class PipePiece : MonoBehaviour
{
    [Header("Puzzle Settings")]
    public PipeRotator.Axis rotationAxis = PipeRotator.Axis.Y;

    [Tooltip("Angles (in degrees) that count as correct — you can have more than one.")]
    public List<float> correctAngles = new List<float>() { 0f };

    public float rotationTolerance = 5f;
    public bool isPartOfPuzzle = true;

    public bool IsCorrectRotation()
    {
        float currentAngle = 0f;

        switch (rotationAxis)
        {
            case PipeRotator.Axis.X: currentAngle = transform.localEulerAngles.x; break;
            case PipeRotator.Axis.Y: currentAngle = transform.localEulerAngles.y; break;
            case PipeRotator.Axis.Z: currentAngle = transform.localEulerAngles.z; break;
        }

        currentAngle = NormalizeAngle(currentAngle);

        foreach (float target in correctAngles)
        {
            float normalizedTarget = NormalizeAngle(target);
            float delta = Mathf.DeltaAngle(currentAngle, normalizedTarget);
            if (Mathf.Abs(delta) <= rotationTolerance)
                return true;
        }

        return false;
    }

    private float NormalizeAngle(float angle)
    {
        angle %= 360f;
        if (angle < 0) angle += 360f;
        return angle;
    }
}
