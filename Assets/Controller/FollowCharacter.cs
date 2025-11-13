using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FollowCharacter : MonoBehaviour
{
    public Transform player;
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float stopDistance = 2f;
    public float runDistance = 6f; // distance à partir de laquelle il court

    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        Vector3 targetPos = new Vector3(player.position.x, transform.position.y, player.position.z);
        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance > stopDistance)
        {
            bool isRunning = distance > runDistance;
            float speed = isRunning ? runSpeed : walkSpeed;

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            Vector3 dir = (targetPos - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.forward = dir;

            // Transition progressive
            float targetVert = isRunning ? 1f : 0.5f; // 0.5 pour walk, 1 pour run
            float currentVert = animator.GetFloat("Vert");
            animator.SetFloat("Vert", Mathf.Lerp(currentVert, targetVert, Time.deltaTime * 5f));

            animator.SetFloat("State", isRunning ? 1f : 0f);
        }
        else
        {
            // Retour progressif à idle
            float currentVert = animator.GetFloat("Vert");
            animator.SetFloat("Vert", Mathf.Lerp(currentVert, 0f, Time.deltaTime * 5f));
            animator.SetFloat("State", 0f);
        }
    }

}
