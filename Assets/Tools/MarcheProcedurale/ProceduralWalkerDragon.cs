using System.Collections;
using UnityEngine;

public class QuadrupedProceduralWalker : MonoBehaviour
{
    [Header("Références IK, en gris dans la scène, c'est ça que les IK vont suivre")]
    [Tooltip("Target IK pour la patte avant droite")]
    public Transform frontRightTarget;
    [Tooltip("Target IK pour la patte avant gauche")]
    public Transform frontLeftTarget;
    [Tooltip("Target IK pour la patte arrière droite")]
    public Transform backRightTarget;
    [Tooltip("Target IK pour la patte arrière gauche")]
    public Transform backLeftTarget;

    [Header("Références 'Home' des Pattes")]
    [Tooltip("Position 'au repos' idéale pour la patte avant droite")]
    public Transform frontRightHome;
    [Tooltip("Position 'au repos' idéale pour la patte avant gauche")]
    public Transform frontLeftHome;
    [Tooltip("Position 'au repos' idéale pour la patte arrière droite")]
    public Transform backRightHome;
    [Tooltip("Position 'au repos' idéale pour la patte arrière gauche")]
    public Transform backLeftHome;

    [Header("Références de transformation du corps")]
    [Tooltip("Transform du corps à déplacer")]
    public Transform bodyTransform;
    [Tooltip("La cible, l'endroit que l'on veut que le corps suive globalement, en bleu dans la scène")]
    public Transform movementGoal;

    [Header("Paramètres de marche")]
    public float moveSpeed = 2f;
    public float stepDistance = 0.8f;
    public float stepSpeed = 2f;
    public float stepHeight = 0.2f;

    [Header("Contrôle et débogage")]
    [Tooltip("Référence du joueur pour la limite de distance")]
    public Transform playerTransform;
    [Tooltip("Distance maximale autorisée entre le joueur et le mesh pour autoriser le déplacement.")]
    public float rangeForMovement = 10f;
    public bool debugLines;

    // Variables privées pour gérer la marche
    private class LegState { public bool moving; } // Pour suivre si une patte est en mouvement

    // Les 4 états liés au fait de si une patte bouge ou pas
    private LegState fr = new LegState();
    private LegState fl = new LegState();
    private LegState br = new LegState();
    private LegState bl = new LegState();

    // Phases de pas alternées (on va faire bouger patte avant droite avec arrière gauche et inversement)
    private enum StepPhase { FR_BL, FL_BR }
    private StepPhase phase = StepPhase.FR_BL; // On commence comme ça (avant droite + arrière gauche)

    void LateUpdate()
    {
        Vector3 move = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow)) move += Vector3.forward;
        if (Input.GetKey(KeyCode.DownArrow)) move += Vector3.back;

        bool canMove = move != Vector3.zero &&
                       Vector3.Distance(bodyTransform.position, playerTransform.position) < rangeForMovement;

        if (canMove)
        {
            movementGoal.position += move.normalized * moveSpeed * Time.deltaTime;
        }

        UpdateStepping();
        UpdateBodyPosition();
    }

    void UpdateStepping()
    {
        if (phase == StepPhase.FR_BL)
        {
            TryStep(frontRightTarget, frontRightHome, fr, () =>
            {
                TryStep(backLeftTarget, backLeftHome, bl, () =>
                {
                    phase = StepPhase.FL_BR;
                });
            });
        }
        else
        {
            TryStep(frontLeftTarget, frontLeftHome, fl, () =>
            {
                TryStep(backRightTarget, backRightHome, br, () =>
                {
                    phase = StepPhase.FR_BL;
                });
            });
        }
    }

    void TryStep(Transform foot, Transform home, LegState state, System.Action onFinished)
    {
        if (state.moving) return;

        float dist = Vector3.Distance(foot.position, home.position);

        if (dist > stepDistance)
        {
            state.moving = true;
            StartCoroutine(MoveFoot(foot, home.position, () =>
            {
                state.moving = false;
                onFinished?.Invoke();
            }));
        }
    }

    IEnumerator MoveFoot(Transform foot, Vector3 targetPosition, System.Action onFinished)
    {
        Vector3 startPos = foot.position;
        float duration = Vector3.Distance(startPos, targetPosition) / stepSpeed;
        if (duration < 0.05f) duration = 0.05f;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            float smooth = t * t * (3 - 2 * t);
            Vector3 pos = Vector3.Lerp(startPos, targetPosition, smooth);

            float h = Mathf.Sin(smooth * Mathf.PI) * stepHeight;
            pos += Vector3.up * h;

            foot.position = pos;

            yield return null;
        }

        foot.position = targetPosition;
        onFinished?.Invoke();
    }

    void UpdateBodyPosition()
    {
        Vector3 avg = (frontRightTarget.position +
                       frontLeftTarget.position +
                       backRightTarget.position +
                       backLeftTarget.position) / 4f;

        bodyTransform.position = new Vector3(
            avg.x,
            bodyTransform.position.y,
            avg.z
        );
    }
}
