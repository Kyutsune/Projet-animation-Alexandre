using UnityEngine;

public class Spin : MonoBehaviour
{
    public float spinSpeed = 720f; 
    private bool spinning = true; // contrôle si le shuriken doit tourner

    void Update()
    {
        if (spinning)
        {
            transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Lorsque le shuriken touche le sol, on arrête la rotation
        if (collision.gameObject.CompareTag("Ground"))
        {
            spinning = false;
        }
    }
}
