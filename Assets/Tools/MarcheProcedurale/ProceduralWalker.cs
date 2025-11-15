using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ToolsSceneRange;

public class ProceduralWalker : MonoBehaviour
{
    [Header("Références IK, en gris dans la scène, c'est ça que les IK vont suivre")]
    [Tooltip("Target IK pour le pied droit")]
    public Transform rightFootTarget;
    [Tooltip("Target IK pour le pied gauche")]
    public Transform leftFootTarget;

    [Header("Références de transformation du corps")]
    [Tooltip("Transform du corps/mesh à déplacer")]
    public Transform bodyTransform;
    [Tooltip("La cible, l'endroit que l'on veut que le corps suive globalement, en bleu dans la scène")]
    public Transform movementGoal;

    [Header("Références 'Home' des Pieds")]
    [Tooltip("Position 'au repos' idéale pour le pied droit")]
    public Transform rightFootHome;
    [Tooltip("Position 'au repos' idéale pour le pied gauche")]
    public Transform leftFootHome;

    [Header("Paramètres de marche")]
    [Tooltip("Vitesse de déplacement du 'movementGoal'")]
    public float moveSpeed = 2f;
    [Tooltip("Distance max avant de déclencher un pas")]
    public float stepDistance = 0.8f;
    [Tooltip("Vitesse de déplacement du pied lors d'un pas")]
    public float stepSpeed = 2f;
    [Tooltip("Hauteur de la courbe du pas")]
    public float stepHeight = 0.1f;

    [Header("Référence du joueur pour la limite de distance")]
    public Transform playerTransform;
    [Tooltip("Distance maximale autorisée entre le joueur et le mesh pour autoriser le déplacement.")]
    public float rangeForMovement = 10.0f;

    [Header("Contrôle et débogage")]
    public bool debugLines = false;

    // Variables privées pour gérer la marche
    private bool _isRightLegTurn = true;
    private bool _rightFootMoving = false;
    private bool _leftFootMoving = false;

    void LateUpdate()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow))
            move += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow))
            move += Vector3.back;

        // On peut bouger si on a un input et que le joueur est proche
        bool canMove = move != Vector3.zero && ToolsSceneRange.IsWithinRange(bodyTransform, playerTransform, rangeForMovement);

        if (canMove)
        {
            // Alors on va bouger la cible de mouvement
            Vector3 displacement = move.normalized * moveSpeed * Time.deltaTime;
            movementGoal.position += displacement;
        }

        // Mettre à jour les pas des pieds
        UpdateStepping();

        // Ainsi que la position du corps vis à vis des pieds
        UpdateBodyPosition();
    }

    void UpdateStepping()
    {
        if (_rightFootMoving || _leftFootMoving) return;

        // Vérifie la distance entre le pied au sol (Target) et sa position idéale (Home)
        float distRight = Vector3.Distance(rightFootTarget.position, rightFootHome.position);
        float distLeft = Vector3.Distance(leftFootTarget.position, leftFootHome.position);

        if (_isRightLegTurn && distRight > stepDistance)
        {
            StartCoroutine(MoveFoot(rightFootTarget, rightFootHome.position, false));
        }
        else if (!_isRightLegTurn && distLeft > stepDistance)
        {
            StartCoroutine(MoveFoot(leftFootTarget, leftFootHome.position, true));
        }
    }

    IEnumerator MoveFoot(Transform footTarget, Vector3 targetPosition, bool isLeftFoot)
    {
        if (isLeftFoot) _leftFootMoving = true;
        else _rightFootMoving = true;

        Vector3 startPos = footTarget.position;
        float timer = 0f;
        // Temps = Distance / Vitesse
        float duration = Vector3.Distance(startPos, targetPosition) / stepSpeed;

        if (duration < 0.01f) duration = 0.01f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            t = t * t * (3f - 2f * t);  // Ease-in / Ease-out

            footTarget.position = Vector3.Lerp(startPos, targetPosition, t);

            float height = Mathf.Sin(t * Mathf.PI) * stepHeight;
            footTarget.position += Vector3.up * height;

            yield return null;
        }

        footTarget.position = targetPosition;

        if (isLeftFoot) _leftFootMoving = false;
        else _rightFootMoving = false;

        _isRightLegTurn = isLeftFoot;
    }



    void UpdateBodyPosition()
    {
        if (rightFootTarget == null || leftFootTarget == null || bodyTransform == null)
            return;

        // On va considérer que le corps doit être centré entre les deux pieds en X et Z
        Vector3 averageFootPosition = (rightFootTarget.position + leftFootTarget.position) / 2f;

        bodyTransform.position = new Vector3(
            averageFootPosition.x,
            bodyTransform.position.y,
            averageFootPosition.z
        );
    }

}