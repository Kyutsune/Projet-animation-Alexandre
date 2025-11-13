using UnityEngine;

[RequireComponent(typeof(Animator))]
public class UnityIKController : MonoBehaviour
{
    public Transform targetHand;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (animator == null)
            return;

        if (targetHand != null)
        {
            // Activer l’IK pour le pied droit
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);

            animator.SetIKPosition(AvatarIKGoal.LeftFoot, targetHand.position);
            animator.SetIKRotation(AvatarIKGoal.LeftFoot, targetHand.rotation);

            // Même chose pour la main droite
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

            animator.SetIKPosition(AvatarIKGoal.RightHand, targetHand.position);
            animator.SetIKRotation(AvatarIKGoal.RightHand, targetHand.rotation);
        }
        else
        {
            // Désactiver l’IK si aucune cible
            animator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 0f);
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0f);
            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0f);
        }
    }
}
