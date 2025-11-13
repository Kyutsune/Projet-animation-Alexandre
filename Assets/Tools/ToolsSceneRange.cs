using UnityEngine;

public class ToolsSceneRange
{
    public static bool IsWithinRange(Transform objectToCheck, Transform referenceObject, float maxDistance)
    {
        float distance = Vector3.Distance(objectToCheck.position, referenceObject.position);
        return distance <= maxDistance;
    }
}
