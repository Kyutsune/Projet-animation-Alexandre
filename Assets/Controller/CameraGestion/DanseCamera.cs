using UnityEngine;

public class DanceCamera : MonoBehaviour
{
    [Header("Cible à suivre")]
    public Transform target;
    [Header("Paramètres de la caméra")]
    [Tooltip("Offset par rapport à la cible.")]
    public Vector3 offset = new Vector3(0, 2, -4);
    [Tooltip("Vitesse de rotation autour de la cible.")]
    public float rotationSpeed = 40f;

    private Camera danceCamera;
    private float currentAngle = 0f;

    void Start()
    {
        danceCamera = GetComponent<Camera>();
        if (danceCamera == null)
        {
            Debug.LogError("DanceCameraOrbit: Aucun composant Camera trouvé sur cet objet.");
        }
    }

    [Header("Zoom")]
    [Tooltip("Active l'oscillation de la distance caméra-cible.")]
    public bool enableZoom = true;
    [Tooltip("Amplitude du rapprochement/éloignement.")]
    public float zoomAmplitude = 0.5f;
    [Tooltip("Vitesse de l'oscillation.")]
    public float zoomSpeed = 1f;
    [Tooltip("Distance minimale entre la caméra et la cible.")]
    public float minDistance = 0.5f;

    void LateUpdate()
    {
        if (!target || !danceCamera || !danceCamera.isActiveAndEnabled) return;

        currentAngle += rotationSpeed * Time.deltaTime;
        float radians = currentAngle * Mathf.Deg2Rad;

        Vector3 rotatedOffset = new Vector3(
            Mathf.Cos(radians) * offset.x - Mathf.Sin(radians) * offset.z,
            offset.y,
            Mathf.Sin(radians) * offset.x + Mathf.Cos(radians) * offset.z
        );

        if (enableZoom)
        {
            // Calcule une variation de distance (sinusoïde) et applique-la le long du vecteur d'offset.
            float baseDistance = rotatedOffset.magnitude;
            float zoomDelta = Mathf.Sin(Time.time * zoomSpeed) * zoomAmplitude;
            float finalDistance = Mathf.Max(minDistance, baseDistance + zoomDelta);
            rotatedOffset = rotatedOffset.normalized * finalDistance;
        }

        transform.position = target.position + rotatedOffset;
        transform.LookAt(target.position + Vector3.up);
    }
}
