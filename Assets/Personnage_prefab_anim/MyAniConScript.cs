using UnityEngine;

public class MyAniConScript : MonoBehaviour
{

    private Animator myAnimator;


    [Header("Inactivité")]
    [Tooltip("Durée avant de déclencher l'animation d'inactivité.")]
    public float idleDelay = 3f;
    private float idleTimer = 0f;
    private bool isDancing = false;
    public void Start()
    {
        myAnimator = GetComponent<Animator>();
        // Debug.Log("MyAniConScript: start => Animator");

    }
    public void Update()
    {
        float verticalInput = Input.GetAxis("Vertical");
        float horizontalInput = Input.GetAxis("Horizontal");

        // --- Détection d'activité ---
        bool playerIsActive = (Mathf.Abs(verticalInput) > 0.01f ||
                               Mathf.Abs(horizontalInput) > 0.01f ||
                               Mathf.Abs(Input.GetAxis("Mouse X")) > 0.01f ||
                               Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.01f ||
                               Input.anyKeyDown ||
                               Input.GetButton("Fire1"));

        if (playerIsActive)
        {
            idleTimer = 0f; // reset du timer
            if (isDancing)
            {
                isDancing = false;
                myAnimator.SetBool("Nothing", false);
            }
        }
        else
        {
            idleTimer += Time.deltaTime;
            if (!isDancing && idleTimer >= idleDelay)
            {
                isDancing = true;
                myAnimator.SetBool("Nothing", true);
            }
        }
        // Debug.Log("Idle Timer: " + idleTimer + " / IdleDelay: " + idleDelay + " / isDancing: " + isDancing);

        // Valeur cible en fonction du shift
        float speedMultiplier = Input.GetKey(KeyCode.LeftShift) ? 2f : 1f;

        float targetVSpeed = verticalInput * speedMultiplier;
        float targetHSpeed = horizontalInput * speedMultiplier;

        // Interpolation douce
        float smoothTime = 2f; // plus grand = plus rapide
        float currentV = myAnimator.GetFloat("vSpeed");
        float currentH = myAnimator.GetFloat("hSpeed");

        float newV = Mathf.MoveTowards(currentV, targetVSpeed, smoothTime * Time.deltaTime);
        float newH = Mathf.MoveTowards(currentH, targetHSpeed, smoothTime * Time.deltaTime);

        myAnimator.SetFloat("vSpeed", newV);
        myAnimator.SetFloat("hSpeed", newH);
    }


    void OnAnimatorIK(int layerIndex)
    {
        if (!myAnimator) return;

        // On va récupérer le forward des hanches/corps pour mettre les pieds dans le bon axe vis à vis du corps
        Transform hips = myAnimator.GetBoneTransform(HumanBodyBones.Hips);
        Vector3 animatedForward = hips != null ? hips.forward : transform.forward;

        // --- PIED DROIT ---
        myAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);
        myAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, 1f);
        Vector3 rightFootPos = myAnimator.GetIKPosition(AvatarIKGoal.RightFoot);
        Ray rayRight = new Ray(rightFootPos + Vector3.up, Vector3.down);

        // Si sur le lancer de rayon on touche le sol, on place le pied à l'endroit et on oriente le pied selon la normale du sol
        if (Physics.Raycast(rayRight, out RaycastHit hitRight, 1.5f))
        {
            myAnimator.SetIKPosition(AvatarIKGoal.RightFoot, hitRight.point);
            myAnimator.SetIKRotation(AvatarIKGoal.RightFoot,
                Quaternion.LookRotation(animatedForward, hitRight.normal));
        }

        // --- PIED GAUCHE, redite du pied droit ---
        myAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);
        myAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, 1f);
        Vector3 leftFootPos = myAnimator.GetIKPosition(AvatarIKGoal.LeftFoot);
        Ray rayLeft = new Ray(leftFootPos + Vector3.up, Vector3.down);

        if (Physics.Raycast(rayLeft, out RaycastHit hitLeft, 1.5f))
        {
            myAnimator.SetIKPosition(AvatarIKGoal.LeftFoot, hitLeft.point);
            myAnimator.SetIKRotation(AvatarIKGoal.LeftFoot,
                Quaternion.LookRotation(animatedForward, hitLeft.normal));
        }
    }
}
