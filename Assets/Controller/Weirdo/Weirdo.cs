using UnityEngine;

public class Weirdo : MonoBehaviour
{
    [Header("Références")]
    public Transform player;

    [Header("IK Main droite")]
    public float reachRange = 5f;
    public float ikBlendSpeed = 3f;

    private Animator animator;
    private Transform targetShuriken;
    private float rightHandIKWeight = 0f;

    private Vector3 rightHandRestPosition;
    private Quaternion rightHandRestRotation;
    private bool restPoseCaptured = false;

    [Header("LookAt paramètres")]
    public float lookAtWeight = 1f;
    public float lookAtBodyWeight = 0.5f;
    public float lookAtHeadWeight = 1f;
    public float lookAtEyesWeight = 1f;
    [Tooltip("Limitation du LookAt (0..1). Mettre entre 0.3 et 0.6 pour des valeurs \"normales\" et 0 pour s'amuser.")]
    public float lookAtClamp = 0.6f;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        FindClosestShuriken();

        float targetWeight = 0f;

        if (targetShuriken != null)
        {
            float dist = Vector3.Distance(transform.position, targetShuriken.position);
            if (dist < reachRange)
                targetWeight = 1f;
        }

        rightHandIKWeight = Mathf.MoveTowards(rightHandIKWeight, targetWeight, Time.deltaTime * ikBlendSpeed);
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null) return;
        
        if (!restPoseCaptured)
        {
            rightHandRestPosition = animator.GetIKPosition(AvatarIKGoal.RightHand);
            rightHandRestRotation = animator.GetIKRotation(AvatarIKGoal.RightHand);
            restPoseCaptured = true;
        }

        // --- LookAt joueur ---
        if (player != null)
        {
            animator.SetLookAtWeight(lookAtWeight, lookAtBodyWeight, lookAtHeadWeight, lookAtEyesWeight, lookAtClamp);
            animator.SetLookAtPosition(player.position);
        }

        // --- IK main droite ---
        if (rightHandIKWeight > 0.01f)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightHandIKWeight);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightHandIKWeight);

            if (targetShuriken != null)
            {
                animator.SetIKPosition(AvatarIKGoal.RightHand, targetShuriken.position);
                animator.SetIKRotation(AvatarIKGoal.RightHand, Quaternion.LookRotation(targetShuriken.position - transform.position));
            }
            else
            {
                Vector3 currentPos = animator.GetIKPosition(AvatarIKGoal.RightHand);
                Quaternion currentRot = animator.GetIKRotation(AvatarIKGoal.RightHand);

                Vector3 newPos = Vector3.Lerp(currentPos, rightHandRestPosition, Time.deltaTime * ikBlendSpeed);
                Quaternion newRot = Quaternion.Slerp(currentRot, rightHandRestRotation, Time.deltaTime * ikBlendSpeed);

                animator.SetIKPosition(AvatarIKGoal.RightHand, newPos);
                animator.SetIKRotation(AvatarIKGoal.RightHand, newRot);
            }
        }
        else
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }
    }

    void FindClosestShuriken()
    {
        GameObject[] shurikens = GameObject.FindGameObjectsWithTag("Shuriken");
        Transform closest = null;
        float minDist = Mathf.Infinity;

        foreach (GameObject s in shurikens)
        {
            float dist = Vector3.Distance(transform.position, s.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = s.transform;
            }
        }

        targetShuriken = closest;
    }
}
