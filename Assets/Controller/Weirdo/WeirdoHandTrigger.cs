using UnityEngine;

public class WeirdoHandTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Shuriken"))
        {
            // Destroy(other.gameObject);
            Debug.Log("Shuriken touché !");
        }
    }
}
