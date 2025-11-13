using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    public Animator targetAnimator;
    public Camera mainCamera;
    public Camera danceCamera;

    void Update()
    {
        if (targetAnimator.GetBool("Nothing"))
        {
            mainCamera.enabled = false;
            danceCamera.enabled = true;
        }
        else
        {
            mainCamera.enabled = true;
            danceCamera.enabled = false;
        }
    }
}