using UnityEngine;

public class CameraPlacement : MonoBehaviour
{
    public Vector3 offset = new Vector3(2f, 2f, -4f);

    void Start()
    {

    }

    void Update()
    {
        transform.localPosition = offset;
    }
}
