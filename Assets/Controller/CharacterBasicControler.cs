using UnityEngine;

public class CharacterBasicControler : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Transform de la caméra attachée au joueur.")]
    public Transform cameraTransform;

    [Header("Mouvement")]
    [Tooltip("Vitesse de déplacement du personnage.")]
    public float speed = 10.0f;
    [Tooltip("Multiplicateur de vitesse lors du sprint.")]
    public float sprintMultiplier = 2.0f;
    [Tooltip("Vitesse de rotation du personnage avec la souris.")]
    public float rotationSpeed = 3.0f;
    [Tooltip("Puissance de poussée lors des collisions.")]
    public float pushPower = 2.0f;

    private CharacterController controller;
    private float verticalRotation = 0f;
    private float verticalVelocity = 0f;
    private float gravity = -9.81f;

    [Header("Tir de Shuriken")]
    [Tooltip("Prefab du shuriken à lancer.")]
    public GameObject shurikenPrefab;
    [Tooltip("Vitesse à laquelle le shuriken est lancé.")]
    public float shurikenSpeed = 30f;
    [Tooltip("Durée de vie du shuriken avant destruction automatique.")]
    public int shurikenLifeTime = 3;
    [Tooltip("Point d'où le shuriken est tiré.")]
    public Transform firePoint;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        // Verrouille et cache le curseur au démarrage
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void Update()
    {
        LockUnlockCursor();
        // --- Mouvement horizontal ---
        Vector3 input = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        if (Input.GetKey(KeyCode.LeftShift))
        {
            input *= sprintMultiplier;
        }
        Vector3 move = transform.TransformDirection(input) * speed;

        // --- Gravité ---
        if (controller.isGrounded)
        {
            verticalVelocity = -0.001f; // petit offset pour rester collé au sol
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }
        move.y = verticalVelocity;

        // --- Déplacement ---
        controller.Move(move * Time.deltaTime);

        // --- Rotation souris ---
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        transform.Rotate(Vector3.up, mouseInput.x * rotationSpeed);

        // --- Caméra ---
        verticalRotation -= mouseInput.y * rotationSpeed;
        verticalRotation = Mathf.Clamp(verticalRotation, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);

        // --- Tir de Shuriken ---
        if (Input.GetButtonDown("Fire1"))
        {
            ThrowShuriken();
        }

    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null || rb.isKinematic)
        {
            return;
        }

        // 3. On évite de pousser vers le bas
        if (hit.moveDirection.y < -0.3f)
        {
            return;
        }

        // On cherche la direction dans laquelle appliquer la poussée et on l'applique
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);
        rb.AddForce(pushDir * pushPower, ForceMode.Impulse);
    }

    void ThrowShuriken()
    {
        if (shurikenPrefab == null || firePoint == null)
        {
            Debug.LogWarning("Prefab ou FirePoint manquant !");
            return;
        }



        Transform hips = transform.Find("mixamorig:Hips");
        Quaternion correction = Quaternion.Euler(-90, 0, 0);
        GameObject shuriken = Instantiate(shurikenPrefab, firePoint.position, cameraTransform.rotation * correction);

        Rigidbody rb = shuriken.GetComponent<Rigidbody>();

        // Direction mixte
        Vector3 bodyForward = transform.forward;          // direction du corps (XZ)
        Vector3 cameraForward = cameraTransform.forward; // direction de la caméra (XYZ)

        // Conserver la rotation horizontale du corps mais Y de la caméra
        Vector3 mixedDir = new Vector3(cameraForward.x, cameraForward.y, cameraForward.z);
        mixedDir.Normalize();
        if (rb != null)
        {
            rb.linearVelocity = mixedDir * shurikenSpeed;
        }

        Destroy(shuriken, shurikenLifeTime);
    }


    void LockUnlockCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

}
