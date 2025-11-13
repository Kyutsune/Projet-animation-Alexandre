using UnityEngine;

public class IKTargetMovement : MonoBehaviour
{
    public float speed = 0.1f;

    [Header("Limite de distance")]
    public Transform GameObjectToLimit;  // L'objet à limiter
    public float maxDistance = 10.0f;    // distance maximale pour autoriser le mouvement

    void Start()
    {

    }

    void Update()
    {
        // Calcul de la direction de mouvement demandée
        Vector3 move = Vector3.zero;

        if (Input.GetKey(KeyCode.LeftArrow))
            move += Vector3.left;
        if (Input.GetKey(KeyCode.RightArrow))
            move += Vector3.right;
        if (Input.GetKey(KeyCode.UpArrow))
            move += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow))
            move += Vector3.back;
        if (Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftShift))
            move += Vector3.up;
        if (Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftShift))
            move += Vector3.down;

            // Si aucun input, ne rien faire
            if (move == Vector3.zero)
                return;

        // Position potentielle après déplacement
        Vector3 newPos = transform.position + move.normalized * speed;

        // Vérifie la distance avec l’objet de référence
        float dist = Vector3.Distance(newPos, GameObjectToLimit.position);

        if (dist <= maxDistance)
        {
            // On déplace si on reste dans la limite
            transform.position = newPos;
        }

    }
}
